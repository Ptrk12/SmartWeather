using Core.Enums; 
using FluentAssertions;
using Interfaces.Repositories;
using Managers;
using Models.responses;
using Models.SqlEntities; 
using Moq;

namespace SmartWeather.Tests
{
    public class AlertLogsManagerTests
    {
        private readonly Mock<IAlertLogsRepository> _alertLogsRepoMock;

        public AlertLogsManagerTests()
        {
            _alertLogsRepoMock = new Mock<IAlertLogsRepository>();
        }

        private AlertLogsManager CreateSut()
        {
            return new AlertLogsManager(_alertLogsRepoMock.Object);
        }

        [Fact]
        public async Task GetDeviceAlertLogs_ShouldReturnMappedLogs_WhenLogsExist()
        {
            var sut = CreateSut();
            int deviceId = 1;
            int pageNumber = 1;
            int pageSize = 10;
            var expectedDate = DateTime.UtcNow;

            var logsFromDb = new List<AlertLog>
            {
                new AlertLog
                {
                    Id = 100,
                    AlertId = 50,
                    TimeStamp = expectedDate,
                    TriggeredValue = 25.5,
                    TriggeredValueThreshold = 20.0,
                    Alert = new Alert
                    {
                        Id = 50,
                        SensorMetric = new SensorMetric
                        {
                            SensorType = SensorType.Temperature 
                        }
                    }
                },
                new AlertLog
                {
                    Id = 101,
                    AlertId = 51,
                    TimeStamp = expectedDate.AddMinutes(-5),
                    TriggeredValue = 80.0,
                    TriggeredValueThreshold = 70.0,
                    Alert = new Alert
                    {
                        Id = 51,
                        SensorMetric = new SensorMetric
                        {
                            SensorType = SensorType.Humidity
                        }
                    }
                }
            };

            var pagedResult = new PagedResult<AlertLog>
            {
                Records = logsFromDb,
                TotalRecords = 2
            };

            _alertLogsRepoMock
                .Setup(x => x.GetDeviceAlertLogs(deviceId, pageNumber, pageSize))
                .ReturnsAsync(pagedResult);

            var result = await sut.GetDeviceAlertLogs(deviceId, pageNumber, pageSize);

            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(2);
            result.Records.Should().HaveCount(2);

            var firstLog = result.Records.First();
            firstLog.Id.Should().Be(100);
            firstLog.AlertId.Should().Be(50);
            firstLog.OccuredDate.Should().Be(expectedDate);
            firstLog.TriggeredValue.Should().Be(25.5);
            firstLog.SensorType.Should().Be("Temperature"); 

            _alertLogsRepoMock.Verify(x => x.GetDeviceAlertLogs(deviceId, pageNumber, pageSize), Times.Once);
        }

        [Fact]
        public async Task GetDeviceAlertLogs_ShouldReturnEmptyList_WhenNoLogsFound()
        {
            var sut = CreateSut();
            int deviceId = 1;

            var emptyPagedResult = new PagedResult<AlertLog>
            {
                Records = new List<AlertLog>(),
                TotalRecords = 0
            };

            _alertLogsRepoMock
                .Setup(x => x.GetDeviceAlertLogs(deviceId, 1, 10))
                .ReturnsAsync(emptyPagedResult);

            var result = await sut.GetDeviceAlertLogs(deviceId, 1, 10);

            result.Should().NotBeNull();
            result.TotalRecords.Should().Be(0);
            result.Records.Should().BeEmpty();

            _alertLogsRepoMock.Verify(x => x.GetDeviceAlertLogs(deviceId, 1, 10), Times.Once);
        }
    }
}