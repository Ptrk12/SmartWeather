using Core.Enums;
using FluentAssertions;
using Interfaces.Managers;
using Interfaces.Repositories;
using Interfaces.Repositories.firebase;
using Managers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Models.firebase;
using Models.requests;
using Models.responses;
using Models.SqlEntities;
using Moq;
using System.Text.Json;
using static Google.Cloud.Firestore.V1.StructuredAggregationQuery.Types.Aggregation.Types;


namespace SmartWeather.Tests
{
    public class DeviceManagerTests
    {
        private readonly Mock<IGenericCrudRepository<Device>> _deviceGeneralRepoMock;
        private readonly Mock<IDeviceRepository> _deviceRepoMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<IFirebaseRepository> _firebaseRepoMock;
        private readonly Mock<IAlertRepository> _alertRepoMock;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<IImageManager> _imageManagerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;

        private readonly DeviceManager _sut;

        public DeviceManagerTests()
        {
            _deviceGeneralRepoMock = new Mock<IGenericCrudRepository<Device>>();
            _deviceRepoMock = new Mock<IDeviceRepository>();
            _configMock = new Mock<IConfiguration>();
            _firebaseRepoMock = new Mock<IFirebaseRepository>();
            _alertRepoMock = new Mock<IAlertRepository>();
            _cacheMock = new Mock<IDistributedCache>();
            _imageManagerMock = new Mock<IImageManager>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();

            _sut = new DeviceManager(
                _deviceGeneralRepoMock.Object,
                _configMock.Object,
                _deviceRepoMock.Object,
                _firebaseRepoMock.Object,
                _alertRepoMock.Object,
                _cacheMock.Object,
                _imageManagerMock.Object,
                _httpClientFactoryMock.Object
            );
        }

        private DeviceManager CreateSut(Dictionary<string, string>? customSettings = null)
        {
            var settings = customSettings ?? new Dictionary<string, string>
        {
            { "Images:StoragePath", "C:\\TestPath" }
        };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            return new DeviceManager(
                _deviceGeneralRepoMock.Object,
                configuration,
                _deviceRepoMock.Object,
                _firebaseRepoMock.Object,
                _alertRepoMock.Object,
                _cacheMock.Object,
                _imageManagerMock.Object,
                _httpClientFactoryMock.Object
            );
        }

        [Fact]
        public async Task DeleteDeviceAsync_ShouldReturnFalse_WhenDeviceNotFound()
        {
            int deviceId = 1;
            _deviceGeneralRepoMock.Setup(x => x.GetByIdAsync(deviceId, null))
                .ReturnsAsync((Device?)null);

            var result = await _sut.DeleteDeviceAsync(deviceId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteDeviceAsync_ShouldReturnTrue_WhenDeviceDeleted()
        {
            var sut = CreateSut();

            int deviceId = 1;
            var device = new Device { Id = deviceId, SerialNumber = "SN123" };

            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(s => s.Value).Returns("C:\\TestPath"); 

            _configMock.Setup(x => x.GetSection("Images")).Returns(configSection.Object);

            _deviceGeneralRepoMock.Setup(x => x.GetByIdAsync(deviceId, null))
                .ReturnsAsync(device);

            _deviceGeneralRepoMock.Setup(x => x.DeleteAsync(device))
                .ReturnsAsync(true);

            _imageManagerMock.Setup(x => x.DeleteImageAsync(device.SerialNumber!));

            var result = await sut.DeleteDeviceAsync(deviceId);

            result.Should().BeTrue();

            _imageManagerMock.Verify(x => x.DeleteImageAsync(device.SerialNumber!), Times.Once);
        }

        [Fact]
        public async Task DeleteDeviceAsync_ShouldRemoveFromCache_WhenSuccess()
        {
            var sut = CreateSut();

            int deviceId = 1;
            var device = new Device { Id = deviceId, SerialNumber = "SN123", GroupId = 10 };

            _deviceGeneralRepoMock.Setup(x => x.GetByIdAsync(deviceId, It.IsAny<Func<IQueryable<Device>, IQueryable<Device>>>())).ReturnsAsync(device);

            _deviceGeneralRepoMock.Setup(x => x.DeleteAsync(device)).ReturnsAsync(true);

            var configSection = new Mock<IConfigurationSection>();
            _configMock.Setup(x => x.GetSection("Images")).Returns(configSection.Object);

            await sut.DeleteDeviceAsync(deviceId);

            _cacheMock.Verify(x => x.RemoveAsync($"group-{device.GroupId}-devices-with-alerts", default), Times.Once);
        }

        [Fact]
        public async Task DeleteDeviceAsync_ShouldNotClearCache_WhenDeleteFails()
        {
            var sut = CreateSut();

            var device = new Device { Id = 1, SerialNumber = "SN1" };

            _deviceGeneralRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<Func<IQueryable<Device>, IQueryable<Device>>>())).ReturnsAsync(device);

            _configMock.Setup(x => x.GetSection("Images:StoragePath")).Returns(new Mock<IConfigurationSection>().Object);

            _deviceGeneralRepoMock.Setup(x => x.DeleteAsync(device)).ReturnsAsync(false); 

            await sut.DeleteDeviceAsync(1);

            _imageManagerMock.Verify(x => x.DeleteImageAsync(It.IsAny<string>()), Times.Never);
            _cacheMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), default), Times.Never);
        }

        [Fact]
        public async Task EditDeviceAsync_ShouldReturnTrue_WhenSucess()
        {
            var deviceId = 1;
            var groupId = 1;

            var sut = CreateSut();

            var deviceGroup = new Group()
            {
                Id = 1,
                Name = "deviceGroup",
                Description = "testDescription"
            };

            var existingDevice = new Device()
            {
                SerialNumber = "A8s7dyB4",
                Location = "testLocation",
                Longitude = 20,
                Latitude = 40,
                GroupId = 1,
                Group = deviceGroup
            };

            var req = new CreateDeviceReq()
            {
                SerialNumber = "edited",
                Location = "edited",
                Longitude = 33,
                Latitude = 44
            };

            _deviceGeneralRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<Func<IQueryable<Device>, IQueryable<Device>>>())).ReturnsAsync(existingDevice);

            _deviceGeneralRepoMock.Setup(x => x.UpdateAsync(existingDevice)).ReturnsAsync(true);

            var result = await sut.EditDeviceAsync(req,deviceId,groupId);

            result.Success.Should().BeTrue();
            existingDevice.SerialNumber.Should().Be(req.SerialNumber);
            existingDevice.Latitude.Should().Be(req.Latitude);
            existingDevice.Longitude.Should().Be(req.Longitude);

            _deviceGeneralRepoMock.Verify(x => x.UpdateAsync(existingDevice), Times.Once);
        }

        [Fact]
        public async Task EditDeviceAsync_ShouldReturnFailure_WhenDeviceNotFound()
        {
            var deviceId = 999;
            var groupId = 10;
            var req = new CreateDeviceReq { SerialNumber = "NewSerial" };
            var sut = CreateSut();

            _deviceGeneralRepoMock.Setup(x => x.GetByIdAsync(deviceId, It.IsAny<Func<IQueryable<Device>, IQueryable<Device>>>()))
                .ReturnsAsync((Device?)null);

            var result = await sut.EditDeviceAsync(req, deviceId, groupId);

            result.Success.Should().BeFalse();
            _deviceGeneralRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Device>()), Times.Never);
        }

        [Fact]
        public async Task EditDeviceAsync_ShouldReturnTrue_AndClearCache_WhenUpdateIsSuccessful_NoImageChange()
        {
            var deviceId = 1;
            var groupId = 10;
            var sut = CreateSut();

            var existingDevice = new Device
            {
                Id = deviceId,
                GroupId = groupId,
                SerialNumber = "OldSerial",
                Location = "OldLocation",
                Latitude = 10,
                Longitude = 10
            };

            var req = new CreateDeviceReq
            {
                SerialNumber = "NewSerial",
                Location = "NewLocation",
                Latitude = 20,
                Longitude = 20,
                ImageFile = null
            };

            _deviceGeneralRepoMock.Setup(x => x.GetByIdAsync(deviceId, It.IsAny<Func<IQueryable<Device>, IQueryable<Device>>>()))
                .ReturnsAsync(existingDevice);

            _deviceGeneralRepoMock.Setup(x => x.UpdateAsync(existingDevice))
                .ReturnsAsync(true);

            var result = await sut.EditDeviceAsync(req, deviceId, groupId);

            result.Success.Should().BeTrue();
            existingDevice.SerialNumber.Should().Be(req.SerialNumber);
            existingDevice.Location.Should().Be(req.Location);
            existingDevice.Latitude.Should().Be(req.Latitude);
            existingDevice.Longitude.Should().Be(req.Longitude);

            _deviceGeneralRepoMock.Verify(x => x.UpdateAsync(existingDevice), Times.Once);
            _cacheMock.Verify(x => x.RemoveAsync($"group-{groupId}-devices-with-alerts", default), Times.Once);
        }

        [Fact]
        public async Task EditDeviceAsync_ShouldReturnFailure_WhenImageIsInvalid()
        {
            var deviceId = 1;
            var groupId = 10;
            var sut = CreateSut();

            var existingDevice = new Device { Id = deviceId, SerialNumber = "SN1" };

            var fileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
            fileMock.Setup(x => x.Length).Returns(100);

            var req = new CreateDeviceReq
            {
                SerialNumber = "SN1",
                ImageFile = fileMock.Object
            };

            _deviceGeneralRepoMock.Setup(x => x.GetByIdAsync(deviceId, It.IsAny<Func<IQueryable<Device>, IQueryable<Device>>>()))
                .ReturnsAsync(existingDevice);

            _imageManagerMock.Setup(x => x.CheckIfImage(req.ImageFile))
                .Returns(false);

            var result = await sut.EditDeviceAsync(req, deviceId, groupId);

            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid image file type");
            _deviceGeneralRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Device>()), Times.Never);
        }

        [Fact]
        public async Task EditDeviceAsync_ShouldUploadImage_AndReturnTrue_WhenImageIsValid()
        {
            var deviceId = 1;
            var groupId = 10;
            var sut = CreateSut();

            var existingDevice = new Device { Id = deviceId, SerialNumber = "SN1", GroupId = groupId };
            var newImagePath = "new/image/path.jpg";

            var fileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
            fileMock.Setup(x => x.Length).Returns(100);

            var req = new CreateDeviceReq
            {
                SerialNumber = "SN1",
                ImageFile = fileMock.Object
            };

            _deviceGeneralRepoMock.Setup(x => x.GetByIdAsync(deviceId, It.IsAny<Func<IQueryable<Device>, IQueryable<Device>>>()))
                .ReturnsAsync(existingDevice);

            _imageManagerMock.Setup(x => x.CheckIfImage(req.ImageFile))
                .Returns(true);

            _imageManagerMock.Setup(x => x.UploadImage(existingDevice, req))
                .ReturnsAsync(newImagePath);

            _deviceGeneralRepoMock.Setup(x => x.UpdateAsync(existingDevice))
                .ReturnsAsync(true);

            var result = await sut.EditDeviceAsync(req, deviceId, groupId);

            result.Success.Should().BeTrue();
            existingDevice.Image.Should().Be(newImagePath);
            _cacheMock.Verify(x => x.RemoveAsync($"group-{groupId}-devices-with-alerts", default), Times.Once);
        }

        [Fact]
        public async Task EditDeviceAsync_ShouldCleanupImage_AndNotClearCache_WhenUpdateFails()
        {
            var deviceId = 1;
            var groupId = 10;
            var sut = CreateSut();

            var existingDevice = new Device
            {
                Id = deviceId,
                SerialNumber = "SN1",
                Image = "old/image.jpg"
            };

            var req = new CreateDeviceReq
            {
                SerialNumber = "SN1",
                ImageFile = null
            };

            _deviceGeneralRepoMock.Setup(x => x.GetByIdAsync(deviceId, It.IsAny<Func<IQueryable<Device>, IQueryable<Device>>>()))
                .ReturnsAsync(existingDevice);

            _deviceGeneralRepoMock.Setup(x => x.UpdateAsync(existingDevice))
                .ReturnsAsync(false);

            var result = await sut.EditDeviceAsync(req, deviceId, groupId);

            result.Success.Should().BeFalse();
            _imageManagerMock.Verify(x => x.DeleteImageAsync(req.SerialNumber), Times.Once);
            _cacheMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), default), Times.Never);
        }

        [Fact]
        public async Task GetDevicesAsync_ShouldFetchFromRepoAndSetCache_WhenCacheIsEmpty()
        {
            var groupId = 10;
            var cacheKey = $"group-{groupId}-devices-with-alerts";

            var devicesFromDb = new List<Device>()
            {
                new Device()
                {
                    Id=1,SerialNumber="SN1",GroupId=groupId,Location="Location1",Status = DeviceStatus.Pending
                },
                new Device()
                {
                    Id=2,SerialNumber="SN2",GroupId=groupId,Location="Location2",Status = DeviceStatus.Active
                }
            };

            _cacheMock.Setup(x => x.GetAsync(cacheKey, default)).ReturnsAsync((byte[])null);

            _deviceRepoMock.Setup(x => x.GetDevicesInGroupAsync(groupId)).ReturnsAsync(devicesFromDb);

            var result = (await _sut.GetDevicesAsync(groupId)).ToList();

            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
            result.First().SerialNumber.Should().Be("SN1");
            result.Should().BeEquivalentTo(devicesFromDb, options => options.ExcludingMissingMembers());

            _deviceRepoMock.Verify(x => x.GetDevicesInGroupAsync(It.IsAny<int>()), Times.Once);

            _cacheMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
        }

        [Fact]
        public async Task GetDevicesAsync_ShouldFetchFromCache_WhenCacheHasData()
        {
            var groupId = 10;
            var cacheKey = $"group-{groupId}-devices-with-alerts";

            var cachedResponse = new List<DeviceResponse>
            {
                new DeviceResponse { Id = 1, SerialNumber = "CachedSN1" }
            };

            var serializedData = JsonSerializer.SerializeToUtf8Bytes(cachedResponse);

            _cacheMock.Setup(x => x.GetAsync(cacheKey, default)).ReturnsAsync(serializedData);

            var result = (await _sut.GetDevicesAsync(groupId)).ToList();

            result.Should().NotBeEmpty();
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().SerialNumber.Should().Be("CachedSN1");

            _deviceRepoMock.Verify(x => x.GetDevicesInGroupAsync(It.IsAny<int>()), Times.Never);

            _cacheMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Never);
        }

        [Fact]
        public async Task AddDeviceAsync_ShouldReturnTrue_IfAddedSuccesfullyAndNoHistorialRecords()
        {
            int groupId = 10;
            var cacheKey = $"group-{groupId}-devices-with-alerts";
            var sut = CreateSut();

            var req = new CreateDeviceReq()
            {
                SerialNumber = "SN1",
                Location = "Location",
                Latitude = 12,
                Longitude = 15
            };

            var device = new Device()
            {
                SerialNumber = req.SerialNumber,
                Location = req.Location,
                GroupId = groupId,
                Longitude = req.Longitude,
                Latitude = req.Latitude
            };

            _deviceRepoMock.Setup(x => x.GetLatestHistoricalRecord(req.SerialNumber)).ReturnsAsync((Device?)null);

            _deviceGeneralRepoMock.Setup(x => x.AddAsync(It.IsAny<Device>())).ReturnsAsync(true);

            var result = await sut.AddDeviceAsync(req, groupId);

            result.Success.Should().BeTrue();
            result.Message.Should().BeNull();

            _deviceGeneralRepoMock.Verify(x => x.AddAsync(It.Is<Device>(d =>
                    d.SerialNumber == req.SerialNumber &&
                    d.Location == req.Location &&
                    d.GroupId == groupId)),Times.Once);

            _cacheMock.Verify(x=>x.RemoveAsync(cacheKey,default),Times.Once);
        }

        [Fact]
        public async Task AddDeviceAsync_ShouldInheritStatus_WhenHistoricalRecordExists()
        {
            int groupId = 10;
            var serialNumber = "SN-HISTORY-123";

            var req = new CreateDeviceReq()
            {
                SerialNumber = serialNumber,
                Location = "Location",
                Latitude = 12,
                Longitude = 15
            };

            var historicalDevice = new Device
            {
                SerialNumber = serialNumber,
                Status = DeviceStatus.Active,
                LastMeasurement = DateTime.UtcNow.AddDays(-1)
            };

            _deviceRepoMock.Setup(x => x.GetLatestHistoricalRecord(req.SerialNumber)).ReturnsAsync(historicalDevice);

            _deviceGeneralRepoMock.Setup(x => x.AddAsync(It.IsAny<Device>())).ReturnsAsync(true);

            var result = await _sut.AddDeviceAsync(req, groupId);

            result.Success.Should().BeTrue();

            _deviceGeneralRepoMock.Verify(x => x.AddAsync(It.Is<Device>(d =>
                    d.SerialNumber == serialNumber &&
                    d.Status == historicalDevice.Status &&
                    d.LastMeasurement == historicalDevice.LastMeasurement
                )), Times.Once);
        }

        [Fact]
        public async Task AddDeviceAsync_ShouldUploadImage_WhenFileIsProvidedAndValid()
        {
            int groupId = 10;
            var sut = CreateSut();
            var expectedImagePath = "uploads/sn123.jpg";

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(x => x.Length).Returns(1024); 

            var req = new CreateDeviceReq
            {
                SerialNumber = "SN123",
                ImageFile = fileMock.Object
            };

            _imageManagerMock.Setup(x => x.CheckIfImage(req.ImageFile)).Returns(true);
            _imageManagerMock.Setup(x => x.UploadImage(null, req)).ReturnsAsync(expectedImagePath);
            _deviceGeneralRepoMock.Setup(x => x.AddAsync(It.IsAny<Device>())).ReturnsAsync(true);

            var result = await sut.AddDeviceAsync(req, groupId);

            result.Success.Should().BeTrue();

            _deviceGeneralRepoMock.Verify(x => x.AddAsync(It.Is<Device>(d =>
                d.Image == expectedImagePath)), Times.Once);
        }

        [Fact]
        public async Task AddDeviceAsync_ShouldDeleteUploadedImage_WhenDatabaseSaveFails()
        {
            int groupId = 10;
            var sut = CreateSut();

            var fileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
            fileMock.Setup(x => x.Length).Returns(1024);

            var req = new CreateDeviceReq
            {
                SerialNumber = "SN_FAIL",
                ImageFile = fileMock.Object
            };

            _imageManagerMock.Setup(x => x.CheckIfImage(req.ImageFile)).Returns(true);
            _imageManagerMock.Setup(x => x.UploadImage(null, req)).ReturnsAsync("temp/path.jpg");

            _deviceGeneralRepoMock.Setup(x => x.AddAsync(It.IsAny<Device>())).ReturnsAsync(false);

            var result = await sut.AddDeviceAsync(req, groupId);

            result.Success.Should().BeFalse();

            _imageManagerMock.Verify(x => x.DeleteImageAsync(req.SerialNumber), Times.Once);
        }

        [Fact]
        public async Task GetDeviceMeasurementAsync_ShouldReturnFilteredMeasurements_WhenCacheIsEmpty()
        {
            var deviceId = 1;
            var serialNumber = "SN123";
            var parameterType = "Temperature";
            var dateFrom = DateTimeOffset.UtcNow.AddMinutes(-30);
            var dateTo = DateTimeOffset.UtcNow;
            var sut = CreateSut();

            var firebaseData = new List<FirebaseDeviceMeasurement>
                {
                    new FirebaseDeviceMeasurement
                    {
                        Timestamp = dateFrom.AddMinutes(10).ToUnixTimeSeconds(),
                        Parameters = new List<Dictionary<string, object>> { new() { { "temperature", 22.5 } } }
                    },
                    new FirebaseDeviceMeasurement
                    {
                        Timestamp = dateFrom.AddMinutes(-10).ToUnixTimeSeconds(), 
                        Parameters = new List<Dictionary<string, object>> { new() { { "temperature", 20.0 } } }
                    },
                    new FirebaseDeviceMeasurement
                    {
                        Timestamp = dateFrom.AddMinutes(15).ToUnixTimeSeconds(), 
                        Parameters = new List<Dictionary<string, object>> { new() { { "humidity", 50.0 } } }
                    }
                };

            _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), default)).ReturnsAsync((byte[])null);
            _deviceRepoMock.Setup(x => x.GetDeviceSerialNumberAsync(deviceId)).ReturnsAsync(serialNumber);
            _firebaseRepoMock.Setup(x => x.GetDeviceMeasurementAsync(serialNumber)).ReturnsAsync(firebaseData);

            var result = await sut.GetDeviceMeasurementAsync(deviceId, parameterType, dateFrom, dateTo);

            result.Parameter.Should().Be(parameterType.ToLower());
            result.Measurements.Should().HaveCount(1); 
            result.Measurements.Values.First().Should().Be(22.5);
            _cacheMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
        }
    }
}
