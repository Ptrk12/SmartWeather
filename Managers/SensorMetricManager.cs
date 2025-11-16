using Core.Constants;
using Core.Enums;
using Interfaces.Managers;
using Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Models.requests;
using Models.responses;
using Models.SqlEntities;

namespace Managers
{
    public class SensorMetricManager : ISensorMetricManager
    {
        private readonly IGenericCrudRepository<SensorMetric> _sensorMetricCrudRepository;
        private readonly IGenericCrudRepository<Device> _deviceCrudRepository;

        public SensorMetricManager(
            IGenericCrudRepository<SensorMetric> sensorMetricCrudRepository,
            IGenericCrudRepository<Device> deviceCrudRepository)
        {
            _sensorMetricCrudRepository = sensorMetricCrudRepository;
            _deviceCrudRepository = deviceCrudRepository;
        }

        private async Task<ExecutionResult> ValidateSensorMetricAsync(CreateSensorMetric req , int deviceId)
        {
            var result = new ExecutionResult();

            var foundDevice = await _deviceCrudRepository.GetByIdAsync(deviceId,x=>x.Include(x=>x.Metrics));

            if (foundDevice == null)
            {
                result.Message = "Device not found";
                return result;
            }

            var deviceMetrics = foundDevice.Metrics;

            if (deviceMetrics.Any(x => x.SensorType.ToString().Equals(req.SensorType, StringComparison.OrdinalIgnoreCase)))
            {
                result.Message = "A sensor metric with the same sensor type name already exists for this device";
                return result;
            }
            if (!Enum.TryParse<SensorType>(req.SensorType, true, out var _))
            {
                result.Message = "Invalid sensor type";
                return result;
            }
            if (!SensorMetricConstants.SensorAndUnits[req.SensorType].Contains(req.Unit))
            {
                result.Message = "Invalid unit for the specified sensor type";
                return result;
            }
            result.Success = true;
            return result;
        }

        public async Task<ExecutionResult> AddSensorMetricAsync(CreateSensorMetric req, int deviceId)
        {
            var result = await ValidateSensorMetricAsync(req, deviceId);

            if(!result.Success)
                return result;

            var sensorMetric = new SensorMetric
            {
                Name = req.Name,
                SensorType = Enum.Parse<SensorType>(req.SensorType),
                Unit = req.Unit,
                DeviceId = deviceId
            };

            result.Success = await _sensorMetricCrudRepository.AddAsync(sensorMetric);

            return result;
        }

        public async Task<ExecutionResult> DeleteSensorMetricAsync(int deviceId, int sensorMetricId)
        {
            var result = new ExecutionResult();

            var foundDevice = await _deviceCrudRepository.GetByIdAsync(deviceId, x => x.Include(x => x.Metrics));

            if(foundDevice == null)
            {
                result.Message = "Device not found";
                return result;
            }

            var sensorMetric = foundDevice.Metrics.FirstOrDefault(x => x.Id == sensorMetricId);

            if (sensorMetric == null)
            {
                result.Message = "Sensor metric not found";
                return result;
            }

            result.Success = await _sensorMetricCrudRepository.DeleteAsync(sensorMetric);
            return result;
        }

        public async Task<ExecutionResult> UpdateSensorMetricAsync(int deviceId, int sensorMetricId, CreateSensorMetric req)
        {
            var result = await ValidateSensorMetricAsync(req, deviceId);
            var foundSensorMetric = await _sensorMetricCrudRepository.GetByIdAsync(sensorMetricId);

            if (foundSensorMetric == null)
            {
                result.Message = "Sensor metric not found";
                return result;
            }

            if (foundSensorMetric.DeviceId != deviceId)
            {
                result.Message = "Sensor metric does not belong to the specified device";
                return result;
            }

            if (!result.Success)
                return result;

            var sensorMetric = new SensorMetric()
            {
                Id = sensorMetricId,
                Name = req.Name,
                SensorType = Enum.Parse<SensorType>(req.SensorType),
                Unit = req.Unit,
                DeviceId = deviceId
            };
            result.Success = await _sensorMetricCrudRepository.UpdateAsync(sensorMetric);
            return result;
        }
    }
}
