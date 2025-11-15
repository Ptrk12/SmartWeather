using Core.Constants;
using Core.Enums;
using Interfaces.Managers;
using Interfaces.Repositories;
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


        public async Task<ExecutionResult> AddSensorMetricAsync(CreateSensorMetric req, int deviceId)
        {
            var result = new ExecutionResult();

            var foundDevice = await _deviceCrudRepository.GetByIdAsync(deviceId);

            if (foundDevice == null)
            {
                result.Message = "Device not found";
                return result;
            }

            if (!Enum.TryParse<SensorType>(req.SensorType, true, out var type))
            {
                result.Message = "Invalid sensor type";
                return result;
            }
            if (!SensorMetricConstants.SensorAndUnits[req.SensorType].Contains(req.Unit))
            {
                result.Message = "Invalid unit for the specified sensor type";
                return result;
            }


            var sensorMetric = new SensorMetric
            {
                Name = req.Name,
                SensorType = type,
                Unit = req.Unit,
                DeviceId = deviceId
            };

            result.Success = await _sensorMetricCrudRepository.AddAsync(sensorMetric);

            return result;
        }
    }
}
