using Core.Enums;
using FluentAssertions;
using Interfaces.Repositories;
using Managers;
using Microsoft.Extensions.Caching.Distributed;
using Models.requests;
using Models.responses;
using Models.SqlEntities;
using Moq;

namespace SmartWeather.Tests
{
    public class AlertManagerTests
    {
        private readonly Mock<IAlertRepository> _alertRepoMock;
        private readonly Mock<IGenericCrudRepository<Alert>> _crudAlertRepoMock;
        private readonly Mock<IGenericCrudRepository<SensorMetric>> _crudSensorMetricRepoMock;
        private readonly Mock<IDistributedCache> _cacheMock;

        public AlertManagerTests()
        {
            _alertRepoMock = new Mock<IAlertRepository>();
            _crudAlertRepoMock = new Mock<IGenericCrudRepository<Alert>>();
            _crudSensorMetricRepoMock = new Mock<IGenericCrudRepository<SensorMetric>>();
            _cacheMock = new Mock<IDistributedCache>();
        }

        private AlertManager CreateSut()
        {
            return new AlertManager(
                _alertRepoMock.Object,
                _crudAlertRepoMock.Object,
                _crudSensorMetricRepoMock.Object,
                _cacheMock.Object
            );
        }


        [Fact]
        public async Task CreateAlertRule_ShouldReturnFailure_WhenConditionIsInvalidEnum()
        {
            var sut = CreateSut();
            var req = new CreateAlertReq { Condition = "InvalidConditionName", ThresholdValue = 10 };

            _crudSensorMetricRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<Func<IQueryable<SensorMetric>, IQueryable<SensorMetric>>>())).ReturnsAsync(new SensorMetric());

            var result = await sut.CreateAlertRule(req, 1, 10);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid alert condition.");
            _crudAlertRepoMock.Verify(x => x.AddAsync(It.IsAny<Alert>()), Times.Never);
        }

        [Fact]
        public async Task CreateAlertRule_ShouldReturnFailure_WhenHumidityThresholdIsInvalid()
        {
            var sut = CreateSut();
            var req = new CreateAlertReq { Condition = "GreaterThan", ThresholdValue = 150 }; 

            var humidityMetric = new SensorMetric { Id = 1, SensorType = SensorType.Humidity };
            _crudSensorMetricRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<Func<IQueryable<SensorMetric>, IQueryable<SensorMetric>>>())).ReturnsAsync(humidityMetric);

            var result = await sut.CreateAlertRule(req, 1, 10);

            result.Success.Should().BeFalse();
            result.Message.Should().Contain("must be between 0 and 100");
        }

        [Fact]
        public async Task CreateAlertRule_ShouldSuccess_AndClearCache_WhenDataIsValid()
        {
            var sut = CreateSut();
            int groupId = 10;
            var req = new CreateAlertReq
            {
                Condition = "GreaterThan", 
                ThresholdValue = 25,
                Name = "High Temp",
                IsEnabled = true
            };

            var metric = new SensorMetric { Id = 1, SensorType = SensorType.Temperature };
            _crudSensorMetricRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<Func<IQueryable<SensorMetric>, IQueryable<SensorMetric>>>())).ReturnsAsync(metric);

            _crudAlertRepoMock.Setup(x => x.AddAsync(It.IsAny<Alert>())).ReturnsAsync(true);

            var result = await sut.CreateAlertRule(req, 1, groupId);

            result.Success.Should().BeTrue();

            _crudAlertRepoMock.Verify(x => x.AddAsync(It.Is<Alert>(a =>
                a.Name == req.Name &&
                a.ThresholdValue == req.ThresholdValue &&
                a.SensorMetricId == 1
            )), Times.Once);

            _cacheMock.Verify(x => x.RemoveAsync($"group-{groupId}-devices-with-alerts", default), Times.Once);
        }

        [Fact]
        public async Task EditAlertRule_ShouldFail_WhenAlertDoesNotBelongToMetric()
        {
            var sut = CreateSut();
            int alertId = 1;
            int metricId = 100;
            int differentMetricId = 200; 

            var existingAlert = new Alert { Id = alertId, SensorMetricId = differentMetricId };
            _crudAlertRepoMock.Setup(x => x.GetByIdAsync(alertId, It.IsAny<Func<IQueryable<Alert>, IQueryable<Alert>>>())).ReturnsAsync(existingAlert);

            var result = await sut.EditAlertRule(new CreateAlertReq(), metricId, alertId, 1);

            result.Success.Should().BeFalse();
            result.Message.Should().Contain("does not belong to the specified sensor metric");
            _crudAlertRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Alert>()), Times.Never);
        }

        [Fact]
        public async Task EditAlertRule_ShouldUpdateFields_AndClearCache_WhenSuccess()
        {
            var sut = CreateSut();
            int alertId = 1;
            int metricId = 10;
            int groupId = 5;

            var req = new CreateAlertReq
            {
                Name = "New Name",
                ThresholdValue = 50,
                Condition = "LessThan",
                IsEnabled = false
            };

            var existingAlert = new Alert
            {
                Id = alertId,
                SensorMetricId = metricId,
                Name = "Old Name"
            };

            var metric = new SensorMetric { Id = metricId, SensorType = SensorType.Temperature };

            _crudAlertRepoMock.Setup(x => x.GetByIdAsync(alertId, It.IsAny<Func<IQueryable<Alert>, IQueryable<Alert>>>())).ReturnsAsync(existingAlert);
            _crudSensorMetricRepoMock.Setup(x => x.GetByIdAsync(metricId, It.IsAny<Func<IQueryable<SensorMetric>, IQueryable<SensorMetric>>>())).ReturnsAsync(metric);
            _crudAlertRepoMock.Setup(x => x.UpdateAsync(existingAlert)).ReturnsAsync(true);

            var result = await sut.EditAlertRule(req, metricId, alertId, groupId);

            result.Success.Should().BeTrue();

            existingAlert.Name.Should().Be("New Name");
            existingAlert.ThresholdValue.Should().Be(50);
            existingAlert.IsEnabled.Should().BeFalse();

            _crudAlertRepoMock.Verify(x => x.UpdateAsync(existingAlert), Times.Once);
            _cacheMock.Verify(x => x.RemoveAsync($"group-{groupId}-devices-with-alerts", default), Times.Once);
        }


        [Fact]
        public async Task DeleteAlertRule_ShouldFail_WhenIdsDoNotMatch()
        {
            var sut = CreateSut();
            int alertId = 1;
            int metricId = 99;

            var alert = new Alert { Id = alertId, SensorMetricId = 55 }; 
            _crudAlertRepoMock.Setup(x => x.GetByIdAsync(alertId, It.IsAny<Func<IQueryable<Alert>, IQueryable<Alert>>>())).ReturnsAsync(alert);

            var result = await sut.DeleteAlertRule(alertId, metricId, 1);

            result.Success.Should().BeFalse();
            result.Message.Should().Contain("does not belong");
            _crudAlertRepoMock.Verify(x => x.DeleteAsync(It.IsAny<Alert>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAlertRule_ShouldSuccess_WhenValid()
        {
            var sut = CreateSut();
            int alertId = 1;
            int metricId = 10;
            int groupId = 5;

            var alert = new Alert { Id = alertId, SensorMetricId = metricId };

            _crudAlertRepoMock.Setup(x => x.GetByIdAsync(alertId, It.IsAny<Func<IQueryable<Alert>, IQueryable<Alert>>>())).ReturnsAsync(alert);
            _crudAlertRepoMock.Setup(x => x.DeleteAsync(alert)).ReturnsAsync(true);

            var result = await sut.DeleteAlertRule(alertId, metricId, groupId);

            result.Success.Should().BeTrue();
            _crudAlertRepoMock.Verify(x => x.DeleteAsync(alert), Times.Once);
            _cacheMock.Verify(x => x.RemoveAsync($"group-{groupId}-devices-with-alerts", default), Times.Once);
        }

        [Fact]
        public async Task GetAlertRuleById_ShouldReturnData_WhenFoundAndMatching()
        {
            var sut = CreateSut();
            int alertId = 1;
            int metricId = 10;

            var alert = new Alert
            {
                Id = alertId,
                SensorMetricId = metricId,
                Name = "Test Alert",
                Condition = AlertCondition.GreaterThan, 
                ThresholdValue = 20,
                IsEnabled = true
            };

            _crudAlertRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<Func<IQueryable<Alert>, IQueryable<Alert>>>())).ReturnsAsync(alert);

            var result = await sut.GetAlertRuleById(metricId, alertId);

            result.Success.Should().BeTrue();

            var data = result.Data as AlertRuleResponse;

            data.Should().NotBeNull();
            data.Name.Should().Be("Test Alert");
            data.SensorMetricId.Should().Be(metricId);
        }

        [Fact]
        public async Task GetAlertRuleById_ShouldReturnFailure_WhenNotFound()
        {
            var sut = CreateSut();

            _crudAlertRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<Func<IQueryable<Alert>, IQueryable<Alert>>>())).ReturnsAsync((Alert?)null);

            var result = await sut.GetAlertRuleById(1, 1);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("Alert rule not found.");
        }
    }
}