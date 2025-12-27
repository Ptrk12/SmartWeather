using Core.Constants; 
using Core.Enums; 
using FluentAssertions;
using Interfaces.Repositories;
using Interfaces.Repositories.firebase;
using Managers;
using Models.firebase;
using Models.requests;
using Models.SqlEntities;
using Moq;

namespace SmartWeather.Tests
{
    public class SensorMetricManagerTests
    {
        private readonly Mock<IGenericCrudRepository<SensorMetric>> _sensorMetricCrudRepoMock;
        private readonly Mock<IGenericCrudRepository<Device>> _deviceCrudRepoMock;
        private readonly Mock<ISensorMetricRepository> _sensorMetricRepoMock;
        private readonly Mock<IFirebaseRepository> _firebaseRepoMock;
        private readonly Mock<IDeviceRepository> _deviceRepoMock;

        public SensorMetricManagerTests()
        {
            _sensorMetricCrudRepoMock = new Mock<IGenericCrudRepository<SensorMetric>>();
            _deviceCrudRepoMock = new Mock<IGenericCrudRepository<Device>>();
            _sensorMetricRepoMock = new Mock<ISensorMetricRepository>();
            _firebaseRepoMock = new Mock<IFirebaseRepository>();
            _deviceRepoMock = new Mock<IDeviceRepository>();
        }

        private SensorMetricManager CreateSut()
        {
            return new SensorMetricManager(
                _sensorMetricCrudRepoMock.Object,
                _deviceCrudRepoMock.Object,
                _sensorMetricRepoMock.Object,
                _firebaseRepoMock.Object,
                _deviceRepoMock.Object
            );
        }


        [Fact]
        public async Task AddSensorMetricAsync_ShouldReturnFailure_WhenDeviceNotFound()
        {
            var sut = CreateSut();
            int deviceId = 1;
            var req = new CreateSensorMetric { SensorType = "Temperature" };

            _deviceCrudRepoMock
                .Setup(x => x.GetByIdAsync(deviceId, It.IsAny<Func<IQueryable<Device>, IQueryable<Device>>>()))
                .ReturnsAsync((Device?)null);

            var result = await sut.AddSensorMetricAsync(req, deviceId);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("Device not found");
        }

        [Fact]
        public async Task AddSensorMetricAsync_ShouldReturnFailure_WhenMetricAlreadyExists()
        {
            var sut = CreateSut();
            int deviceId = 1;
            var req = new CreateSensorMetric { SensorType = "Temperature", Unit = "°C" };

            var existingDevice = new Device
            {
                Id = deviceId,
                Metrics = new List<SensorMetric>
                {
                    new SensorMetric { SensorType = SensorType.Temperature }
                }
            };

            _deviceCrudRepoMock
                .Setup(x => x.GetByIdAsync(deviceId, It.IsAny<Func<IQueryable<Device>, IQueryable<Device>>>()))
                .ReturnsAsync(existingDevice);

            var result = await sut.AddSensorMetricAsync(req, deviceId);

            result.Success.Should().BeFalse();
            result.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task AddSensorMetricAsync_ShouldReturnSuccess_WhenDataIsValid()
        {
            var sut = CreateSut();
            int deviceId = 1;
            var req = new CreateSensorMetric { Name = "My Temp", SensorType = SensorType.Temperature.ToString(), Unit = "°C" };

            var device = new Device { Id = deviceId, Metrics = new List<SensorMetric>() };

            _deviceCrudRepoMock
                .Setup(x => x.GetByIdAsync(deviceId, It.IsAny<Func<IQueryable<Device>, IQueryable<Device>>>()))
                .ReturnsAsync(device);

            _sensorMetricCrudRepoMock
                .Setup(x => x.AddAsync(It.IsAny<SensorMetric>()))
                .ReturnsAsync(true);

            var result = await sut.AddSensorMetricAsync(req, deviceId);

            result.Success.Should().BeTrue();

            _sensorMetricCrudRepoMock.Verify(x => x.AddAsync(It.Is<SensorMetric>(m =>
                m.Name == req.Name &&
                m.SensorType == SensorType.Temperature &&
                m.DeviceId == deviceId
            )), Times.Once);
        }


        [Fact]
        public async Task GetSensorMetricsAsync_ShouldSplitDustMetric_IntoPM25_And_PM10()
        {
            var sut = CreateSut();
            int deviceId = 1;
            string serialNumber = "SN123";

            var metricsFromDb = new List<SensorMetric>
            {
                new SensorMetric { Id = 1, SensorType = SensorType.Dust, Unit = "µg/m³", Name = "Air Quality" }
            };

            _sensorMetricRepoMock.Setup(x => x.GetAllSensorMetricAsync(deviceId)).ReturnsAsync(metricsFromDb);
            _deviceRepoMock.Setup(x => x.GetDeviceSerialNumberAsync(deviceId)).ReturnsAsync(serialNumber);

            var firebaseData = new FirebaseDeviceMeasurement 
            {
                Parameters = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "pm2_5", 15.5 } },
                    new Dictionary<string, object> { { "pm10", 30.0 } }
                }
            };

            _firebaseRepoMock.Setup(x => x.GetLatestDeviceMeasurementAsync(serialNumber)).ReturnsAsync(firebaseData);

            var result = (await sut.GetSensorMetricsAsync(deviceId)).ToList();

            result.Should().HaveCount(2); 

            var pm25 = result.FirstOrDefault(x => x.SensorType == "PM2_5");
            pm25.Should().NotBeNull();
            pm25.LatestMeasurement.Should().Be(15.5);

            var pm10 = result.FirstOrDefault(x => x.SensorType == "PM10");
            pm10.Should().NotBeNull();
            pm10.LatestMeasurement.Should().Be(30.0);
        }

        [Fact]
        public async Task GetSensorMetricsAsync_ShouldReturnStandardMetric_WithMeasurement()
        {
            var sut = CreateSut();
            int deviceId = 1;
            string serialNumber = "SN123";

            var metricsFromDb = new List<SensorMetric>
            {
                new SensorMetric { Id = 10, SensorType = SensorType.Temperature, Unit = "°C" }
            };

            _sensorMetricRepoMock.Setup(x => x.GetAllSensorMetricAsync(deviceId)).ReturnsAsync(metricsFromDb);
            _deviceRepoMock.Setup(x => x.GetDeviceSerialNumberAsync(deviceId)).ReturnsAsync(serialNumber);

            var firebaseData = new FirebaseDeviceMeasurement
            {
                Parameters = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "temperature", 22.4 } }
                }
            };
            _firebaseRepoMock.Setup(x => x.GetLatestDeviceMeasurementAsync(serialNumber)).ReturnsAsync(firebaseData);

            var result = (await sut.GetSensorMetricsAsync(deviceId)).ToList();

            result.Should().HaveCount(1);
            result[0].SensorType.Should().Be("Temperature");
            result[0].LatestMeasurement.Should().Be(22.4);
        }


        [Fact]
        public async Task DeleteSensorMetricAsync_ShouldReturnFailure_WhenMetricNotFoundInDevice()
        {
            var sut = CreateSut();
            int deviceId = 1;
            int metricId = 999;

            var device = new Device
            {
                Id = deviceId,
                Metrics = new List<SensorMetric>
                {
                    new SensorMetric { Id = 10 } 
                }
            };

            _deviceCrudRepoMock
                .Setup(x => x.GetByIdAsync(deviceId, It.IsAny<Func<IQueryable<Device>, IQueryable<Device>>>()))
                .ReturnsAsync(device);

            var result = await sut.DeleteSensorMetricAsync(deviceId, metricId);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("Sensor metric not found");
            _sensorMetricCrudRepoMock.Verify(x => x.DeleteAsync(It.IsAny<SensorMetric>()), Times.Never);
        }

        [Fact]
        public async Task DeleteSensorMetricAsync_ShouldSuccess_WhenMetricExists()
        {
            var sut = CreateSut();
            int deviceId = 1;
            int metricId = 10;

            var metricToDelete = new SensorMetric { Id = metricId };
            var device = new Device
            {
                Id = deviceId,
                Metrics = new List<SensorMetric> { metricToDelete }
            };

            _deviceCrudRepoMock
                .Setup(x => x.GetByIdAsync(deviceId, It.IsAny<Func<IQueryable<Device>, IQueryable<Device>>>()))
                .ReturnsAsync(device);

            _sensorMetricCrudRepoMock.Setup(x => x.DeleteAsync(metricToDelete)).ReturnsAsync(true);

            var result = await sut.DeleteSensorMetricAsync(deviceId, metricId);

            result.Success.Should().BeTrue();
            _sensorMetricCrudRepoMock.Verify(x => x.DeleteAsync(metricToDelete), Times.Once);
        }


        [Fact]
        public async Task UpdateSensorMetricAsync_ShouldFail_WhenMetricBelongsToOtherDevice()
        {
            var sut = CreateSut();
            int deviceId = 1;
            int otherDeviceId = 2;
            int metricId = 10;
            var req = new CreateSensorMetric { SensorType = "Temperature", Unit = "°C" };

            var existingMetric = new SensorMetric { Id = metricId, DeviceId = otherDeviceId };

            var device = new Device { Id = deviceId, Metrics = new List<SensorMetric>() };
            _deviceCrudRepoMock.Setup(x => x.GetByIdAsync(deviceId, It.IsAny<Func<IQueryable<Device>, IQueryable<Device>>>())).ReturnsAsync(device);

            _sensorMetricCrudRepoMock.Setup(x => x.GetByIdAsync(metricId, It.IsAny<Func<IQueryable<SensorMetric>, IQueryable<SensorMetric>>>())).ReturnsAsync(existingMetric);

            var result = await sut.UpdateSensorMetricAsync(deviceId, metricId, req);

            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Sensor metric does not belong to the specified device");
        }
    }
}