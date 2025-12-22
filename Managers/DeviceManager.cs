using Core.Extensions;
using Interfaces.Managers;
using Interfaces.Repositories;
using Interfaces.Repositories.firebase;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Models.requests;
using Models.responses;
using Models.SqlEntities;
using System.Globalization;

namespace Managers
{
    public class DeviceManager : IDeviceManager
    {
        private readonly IGenericCrudRepository<Device> _deviceGeneralRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IConfiguration _configuration;
        private readonly IFirebaseRepository _firebaseRepository;
        private readonly IAlertRepository _alertRepository;
        private readonly IDistributedCache _cache;
        private readonly IImageManager _imageManager;
        public DeviceManager(
            IGenericCrudRepository<Device> deviceGeneralRepository,
            IConfiguration configuration,
            IDeviceRepository deviceRepository,
            IFirebaseRepository firebaseRepository,
            IAlertRepository alertRepository,
            IDistributedCache cache,
            IImageManager imageManager
            )
        {
            _deviceGeneralRepository = deviceGeneralRepository;
            _configuration = configuration;
            _deviceRepository = deviceRepository;
            _firebaseRepository = firebaseRepository;
            _alertRepository = alertRepository;
            _cache = cache;
            _imageManager = imageManager;
        }



        public async Task<bool> DeleteDeviceAsync(int deviceId)
        {
            var device = await _deviceGeneralRepository.GetByIdAsync(deviceId);
            if (device == null)
                return false;

            var rootPath = _configuration.GetSection("Images").GetValue<string>("StoragePath");
            var deviceFolder = Path.Combine(rootPath, device.SerialNumber);
            if (Directory.Exists(deviceFolder))
            {
                Directory.Delete(deviceFolder, true);
            }
            var isSuccess = await _deviceGeneralRepository.DeleteAsync(device);

            if (isSuccess)
            {
                _imageManager.DeleteImageAsync(device.SerialNumber);
                 await _cache.RemoveAsync($"group-{device.GroupId}-devices-with-alerts");
            }

            return isSuccess;
        }

        public async Task<IEnumerable<AlertStatusResponse>> GetDeviceAlerts(string deviceSerialNumber, int deviceId)
        {
            var result = new List<AlertStatusResponse>();
            var latestMeasurement = await _firebaseRepository.GetLatestDeviceMeasurementAsync(deviceSerialNumber);

            if (latestMeasurement == null)
            {
                return result;
            }

            var deviceAlertRules = await _alertRepository.GetDeviceAlertRules(deviceId);

            if (deviceAlertRules.Any())
            {
                foreach (var alertRule in deviceAlertRules)
                {
                    if (!alertRule.IsEnabled)
                        continue;

                    var parameterValue = latestMeasurement.Parameters.FirstOrDefault(p => p.ContainsKey(alertRule.SensorMetric.SensorType.ToString().ToLower()));

                    if (parameterValue != null && parameterValue.TryGetValue(alertRule.SensorMetric.SensorType.ToString().ToLower(), out var valueObj) && double.TryParse(valueObj.ToString(), out var value))
                    {
                        string message = string.Empty;
                        string sensorTypeAlert = alertRule.SensorMetric.SensorType.ToString();

                        switch (alertRule.Condition)
                        {
                            case Core.Enums.AlertCondition.GreaterThan:
                                if (value > alertRule.ThresholdValue)
                                {
                                    message = $"{value} exceeds threshold of {alertRule.ThresholdValue}.";
                                    result.Add(new AlertStatusResponse()
                                    {
                                        AlertMessage = message,
                                        IsAlert = true,
                                        SensorType = sensorTypeAlert
                                    });
                                }
                                break;

                            case Core.Enums.AlertCondition.LessThan:
                                if (value < alertRule.ThresholdValue)
                                {
                                    message = $"{value} is below threshold of {alertRule.ThresholdValue}.";
                                    result.Add(new AlertStatusResponse()
                                    {
                                        AlertMessage = message,
                                        IsAlert = true,
                                        SensorType = sensorTypeAlert
                                    });
                                }
                                break;
                        }
                    }
                }
            }
            return result;
        }

        public async Task<IEnumerable<DeviceResponse>> GetDevicesAsync(int groupId)
        {
            string cacheKey = $"group-{groupId}-devices-with-alerts";

            return await _cache.GetOrSetAsync(cacheKey, async () =>
            {
                var devices = await _deviceRepository.GetDevicesInGroupAsync(groupId);

                if (!devices.Any())
                    return Enumerable.Empty<DeviceResponse>();

                var tasks = devices.Select(async device =>
                {
                    var status = await GetDeviceAlerts(device.SerialNumber, device.Id);

                    return new DeviceResponse
                    {
                        Id = device.Id,
                        SerialNumber = device.SerialNumber,
                        Location = device.Location,
                        Image = device.Image,
                        Status = device.Status.ToString(),
                        LastMeasurement = device.LastMeasurement,
                        AlertStatuses = status,
                        Latitude = device.Latitude,
                        Longitude = device.Longitude
                    };
                });

                return await Task.WhenAll(tasks);
            },new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)});       
        }

        public async Task<ExecutionResult> EditDeviceAsync(CreateDeviceReq req, int deviceId, int groupId)
        {
            var result = new ExecutionResult();
            var device = await _deviceGeneralRepository.GetByIdAsync(deviceId);
            if (device == null)
                return result;

            var rootPath = _configuration.GetSection("Images").GetValue<string>("StoragePath");
            var oldSerial = device.SerialNumber;
            var newSerial = req.SerialNumber;
            var oldFolder = Path.Combine(rootPath, oldSerial);
            var newFolder = Path.Combine(rootPath, newSerial);

            bool serialChanged = !string.Equals(oldSerial, newSerial, StringComparison.OrdinalIgnoreCase);

            if (serialChanged && (req.ImageFile == null || req.ImageFile.Length == 0))
            {
                if (Directory.Exists(oldFolder))
                {
                    if (Directory.Exists(newFolder))
                        Directory.Delete(newFolder, true);

                    Directory.Move(oldFolder, newFolder);
                    if (!string.IsNullOrEmpty(device.Image))
                        device.Image = Path.Combine(newSerial, Path.GetFileName(device.Image)).Replace("\\", "/");
                }
            }
            if (req.ImageFile != null && req.ImageFile.Length > 0)
            {
                if (!_imageManager.CheckIfImage(req.ImageFile))
                {
                    result.Message = "Invalid image file type. Only JPEG and PNG are allowed.";
                    return result;
                }

                if (serialChanged && Directory.Exists(oldFolder))
                {
                    device.Image = await _imageManager.UploadImage(device, req);
                    Directory.Delete(oldFolder, true);
                }
                else
                {
                    device.Image = await _imageManager.UploadImage(device, req);
                }
            }

            device.SerialNumber = newSerial;
            device.Location = req.Location;
            device.Latitude = req.Latitude;
            device.Longitude = req.Longitude;

            var isSuccess = await _deviceGeneralRepository.UpdateAsync(device);

            if (!isSuccess)
            {
                if (!string.IsNullOrEmpty(device.Image))
                    _imageManager.DeleteImageAsync(req.SerialNumber);
            }
            else
                await _cache.RemoveAsync($"group-{groupId}-devices-with-alerts");

            result.Success = isSuccess;

            return result;
        }


        public async Task<ExecutionResult> AddDeviceAsync(CreateDeviceReq req, int groupId)
        {
            var result = new ExecutionResult();

            var device = new Device()
            {
                SerialNumber = req.SerialNumber,
                Location = req.Location,
                GroupId = groupId,
                Longitude = req.Longitude,
                Latitude = req.Latitude
            };

            if (req.ImageFile != null && req.ImageFile.Length > 0)
            {
                if (!_imageManager.CheckIfImage(req.ImageFile))
                {
                    result.Message = "Invalid image file type. Only JPEG and PNG are allowed.";
                    return result;
                }
                device.Image = await _imageManager.UploadImage(null, req);
            }

            var historicalDevice = await _deviceRepository.GetLatestHistoricalRecord(req.SerialNumber);

            if (historicalDevice != null)
            {
                device.Status = historicalDevice.Status;
                device.LastMeasurement = historicalDevice.LastMeasurement;
            }

            var isSuccess = await _deviceGeneralRepository.AddAsync(device);

            if (isSuccess)
                await _cache.RemoveAsync($"group-{groupId}-devices-with-alerts");
            else
            {
                if(!string.IsNullOrEmpty(device.Image))
                    _imageManager.DeleteImageAsync(req.SerialNumber);
            }
                

            result.Success = isSuccess;
            return result;
        }

        public async Task<MeasurementResponse> GetDeviceMeasurementAsync(int deviceId, string parameterType, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            var cacheKey = $"device-{deviceId}-measurements";
            parameterType = parameterType.ToLower();

            var deviceMeasurements = await _cache.GetOrSetAsync(cacheKey, async () =>
            {
                var deviceSerialNumber = await _deviceRepository.GetDeviceSerialNumberAsync(deviceId);
                if (string.IsNullOrEmpty(deviceSerialNumber))
                    return null; 

                var firebaseData = await _firebaseRepository.GetDeviceMeasurementAsync(deviceSerialNumber);            
                return firebaseData; 

            }, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

            var result = new MeasurementResponse()
            {
                Parameter = parameterType,
                Measurements = new Dictionary<DateTimeOffset, double>()
            };

            if(deviceMeasurements == null || !deviceMeasurements.Any())
                return result;

            foreach (var measurement in deviceMeasurements)
            {
                var measurementTime = DateTimeOffset.FromUnixTimeSeconds(measurement.Timestamp);

                if ((dateFrom.HasValue && measurementTime < dateFrom.Value) || (dateTo.HasValue && measurementTime > dateTo.Value))
                {
                    continue; 
                }
                var deviceParameter = measurement.Parameters.FirstOrDefault(x => x.ContainsKey(parameterType));

                if (deviceParameter != null && double.TryParse(deviceParameter[parameterType].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValue))
                {
                    result.Measurements[measurementTime] = parsedValue;
                }
            }
            result.Measurements = result.Measurements
                                        .OrderByDescending(kv => kv.Key)
                                        .ToDictionary(kv => kv.Key, kv => kv.Value);
            return result;
        }
    }
}
