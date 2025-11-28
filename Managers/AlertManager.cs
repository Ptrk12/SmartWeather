using Core.Enums;
using Interfaces.Managers;
using Interfaces.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Models.requests;
using Models.responses;
using Models.SqlEntities;

namespace Managers
{
    public class AlertManager : IAlertManager
    {
        private readonly IAlertRepository _alertRepository;
        private readonly IGenericCrudRepository<Alert> _crudAlertRepository;
        private readonly IGenericCrudRepository<SensorMetric> _crudSensorMetricRepository;
        private readonly IDistributedCache _cache;

        public AlertManager(IAlertRepository alertRepository, IGenericCrudRepository<Alert> crudAlertRepository, IGenericCrudRepository<SensorMetric> crudSensorMetricRepository, IDistributedCache cache)
        {
            _alertRepository = alertRepository;
            _crudAlertRepository = crudAlertRepository;
            _crudSensorMetricRepository = crudSensorMetricRepository;
            _cache = cache;
        }

        public async Task<ExecutionResult> DeleteAlertRule(int alertId, int sensorMetricId, int groupId)
        {
            var result = new ExecutionResult();
            var foundAlert = await _crudAlertRepository.GetByIdAsync(alertId);

            if (foundAlert != null && foundAlert.SensorMetricId != sensorMetricId)
            {
                result.Message = "Alert rule does not belong to the specified sensor metric.";
                return result;
            }

            if (foundAlert != null)
            {
                var isSuccess = await _crudAlertRepository.DeleteAsync(foundAlert);
                result.Success = isSuccess;
            }

            if(result.Success)
                await _cache.RemoveAsync($"group-{groupId}-devices-with-alerts");

            return result;
        }

        public async Task<ExecutionResult> EditAlertRule(CreateAlertReq req, int sensorMetricId, int alertId, int groupId)
        {
            var result = new ExecutionResult();

            var foundAlert = await _crudAlertRepository.GetByIdAsync(alertId);

            if (foundAlert != null && foundAlert.SensorMetricId != sensorMetricId)
            {
                result.Message = "Alert rule does not belong to the specified sensor metric.";
                return result;
            }
            var sensorMetric = await _crudSensorMetricRepository.GetByIdAsync(sensorMetricId);

            var validateResult = ValidateAlertCondition(req, sensorMetric);

            result.Success = validateResult.Success;
            result.Message = validateResult.Message;

            if (!result.Success)
                return result;

            if (foundAlert != null)
            {
                foundAlert.ThresholdValue = req.ThresholdValue;
                foundAlert.Condition = Enum.Parse<AlertCondition>(req.Condition);
                foundAlert.Name = req.Name;
                foundAlert.IsEnabled = req.IsEnabled;
            }

            var isSuccess = await _crudAlertRepository.UpdateAsync(foundAlert);

            if(isSuccess)
                await _cache.RemoveAsync($"group-{groupId}-devices-with-alerts");

            result.Success = isSuccess;
            return result;
        }

        private ExecutionResult ValidateAlertCondition(CreateAlertReq req, SensorMetric? sensorMetric)
        {
            var result = new ExecutionResult();

            if (Enum.TryParse<AlertCondition>(req.Condition, out var condition) == false)
            {
                result.Message = "Invalid alert condition.";
                return result;
            }

            if (sensorMetric != null)
            {
                if (sensorMetric.SensorType == SensorType.Humidity && (req.ThresholdValue < 0 || req.ThresholdValue > 100))
                {
                    result.Message = "Threshold value for Humidity must be between 0 and 100.";
                    return result;
                }
            }
            result.Success = true;
            return result;
        }

        public async Task<ExecutionResult> CreateAlertRule(CreateAlertReq req, int sensorMetricId, int groupId)
        {
            var result = new ExecutionResult();
            var sensorMetric = await _crudSensorMetricRepository.GetByIdAsync(sensorMetricId);

            var validateResult = ValidateAlertCondition(req, sensorMetric);

            result.Success = validateResult.Success;
            result.Message = validateResult.Message;

            if (!result.Success)
                return result;

            var alert = new Alert()
            {
                Name = req.Name,
                Condition = Enum.Parse<AlertCondition>(req.Condition),
                ThresholdValue = req.ThresholdValue,
                SensorMetricId = sensorMetricId,
                IsEnabled = req.IsEnabled
            };

            var isSuccess = await _crudAlertRepository.AddAsync(alert);

            if(isSuccess)
                await _cache.RemoveAsync($"group-{groupId}-devices-with-alerts");

            result.Success = isSuccess;
            return result;
        }

        public async Task<IEnumerable<AlertRuleResponse>> GetDeviceAlertRules(int deviceId)
        {
            var alerts = await _alertRepository.GetDeviceAlertRules(deviceId);
            var result = alerts.Select(a => new AlertRuleResponse
            {
                Id = a.Id,
                Name = a.Name,
                Condition = a.Condition.ToString(),
                ThresholdValue = a.ThresholdValue,
                IsEnabled = a.IsEnabled,
                SensorMetricId = a.SensorMetricId
            });
            return result;
        }

        public async Task<ExecutionResult> GetAlertRuleById(int sensorMetricId, int alertId)
        {
            var result = new ExecutionResult();

            var alert = await _crudAlertRepository.GetByIdAsync(alertId);

            if (alert != null && alert.SensorMetricId != sensorMetricId)
            {
                result.Message = "Alert rule does not belong to the specified sensor metric.";
                return result;
            }

            if (alert == null)
            {
                result.Message = "Alert rule not found.";
                return result;
            }
                
            var resultAlert = new AlertRuleResponse()
            {
                Condition = alert.Condition.ToString(),
                Id = alert.Id,
                IsEnabled = alert.IsEnabled,
                Name = alert.Name,
                SensorMetricId = alert.SensorMetricId,
                ThresholdValue = alert.ThresholdValue
            };

            result.Data = resultAlert;
            result.Success = true;

            return result;
        }
    }
}
