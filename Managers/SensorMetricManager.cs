using Core.Constants;
using Core.Enums;
using Interfaces.Managers;
using Interfaces.Repositories;
using Interfaces.Repositories.firebase;
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
        private readonly ISensorMetricRepository _sensorMetricRepository;
        private readonly IFirebaseRepository _firebaseRepository;
        private readonly IDeviceRepository _deviceRepository;

        public SensorMetricManager(
            IGenericCrudRepository<SensorMetric> sensorMetricCrudRepository,
            IGenericCrudRepository<Device> deviceCrudRepository,
            ISensorMetricRepository sensorMetricRepository,
            IFirebaseRepository firebaseRepository,
            IDeviceRepository deviceRepository)
        {
            _sensorMetricCrudRepository = sensorMetricCrudRepository;
            _deviceCrudRepository = deviceCrudRepository;
            _sensorMetricRepository = sensorMetricRepository;
            _firebaseRepository = firebaseRepository;
            _deviceRepository = deviceRepository;
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
            if (!Enum.TryParse<SensorType>(req.SensorType, false, out var _))
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

        public async Task<IEnumerable<SensorMetricResponse>> GetSensorMetricsAsync(int deviceId)
        {
            var sensorMetrics = await _sensorMetricRepository.GetAllSensorMetricAsync(deviceId);

            if (!sensorMetrics.Any())
                return Enumerable.Empty<SensorMetricResponse>();

            var result = new List<SensorMetricResponse>();

            var deviceSerialNumber = await _deviceRepository.GetDeviceSerialNumberAsync(deviceId);

            foreach (var metric in sensorMetrics)
            {
                var item = new SensorMetricResponse()
                {
                    Id = metric.Id,
                    Name = metric.Name,
                    SensorType = metric.SensorType.ToString(),
                    Unit = metric.Unit
                };

                if (!string.IsNullOrEmpty(deviceSerialNumber))
                {
                    var latestMeasurements = await _firebaseRepository.GetLatestDeviceMeasurementAsync(deviceSerialNumber);

                    if (latestMeasurements != null)
                    {
                        var key = metric.SensorType.ToString().ToLower();
                        var measurementDict = latestMeasurements.Parameters.FirstOrDefault(x => x.ContainsKey(key));
                        
                        if(measurementDict != null && measurementDict.TryGetValue(key,out var outValue))
                        {
                            if(double.TryParse(outValue.ToString(),out var parsedValue))
                            {
                                item.LatestMeasurement = parsedValue;
                            }
                        }
                    }
                }
                result.Add(item);
            }
           
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
