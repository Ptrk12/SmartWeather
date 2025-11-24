using Interfaces.Managers;
using Interfaces.Repositories;
using Interfaces.Repositories.firebase;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Models.requests;
using Models.responses;
using Models.SqlEntities;

namespace Managers
{
    public class DeviceManager : IDeviceManager
    {
        private readonly IGenericCrudRepository<Device> _deviceGeneralRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IConfiguration _configuration;
        private readonly IFirebaseRepository _firebaseRepository;

        public DeviceManager(
            IGenericCrudRepository<Device> deviceGeneralRepository,
            IConfiguration configuration,
            IDeviceRepository deviceRepository,
            IFirebaseRepository firebaseRepository)
        {
            _deviceGeneralRepository = deviceGeneralRepository;
            _configuration = configuration;
            _deviceRepository = deviceRepository;
            _firebaseRepository = firebaseRepository;
        }

        private bool CheckIfImage(IFormFile? file)
        {
            var allowedMimeTypes = new[] { "image/jpeg", "image/png" };

            if (file != null && !allowedMimeTypes.Contains(file.ContentType))
            {
                return false;
            }
            return true;
        }

        private async Task<string> UploadImage(Device? device, CreateDeviceReq req)
        {
            var rootPath = _configuration.GetSection("Images").GetValue<string>("StoragePath");
            var deviceFolder = Path.Combine(rootPath, req.SerialNumber);

            if (!Directory.Exists(deviceFolder))
                Directory.CreateDirectory(deviceFolder);

            if (device != null && !string.IsNullOrEmpty(device.Image))
            {
                var oldImagePath = Path.Combine(rootPath, device.Image);
                if (File.Exists(oldImagePath))
                    File.Delete(oldImagePath);
            }

            var extension = Path.GetExtension(req.ImageFile.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(deviceFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await req.ImageFile.CopyToAsync(stream);
            }

            return Path.Combine(req.SerialNumber, fileName).Replace("\\", "/");
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
            return await _deviceGeneralRepository.DeleteAsync(device);
        }

        public async Task<IEnumerable<DeviceResponse>> GetDevicesAsync(int groupId)
        {
            var devices = await _deviceRepository.GetDevicesInGroupAsync(groupId);

            if (!devices.Any())
                return Enumerable.Empty<DeviceResponse>();

            var result = new List<DeviceResponse>();
            //metrics later
            foreach (var device in devices)
            {
                result.Add(new DeviceResponse()
                {
                    Id = device.Id,
                    SerialNumber = device.SerialNumber,
                    Location = device.Location,
                    Image = device.Image,
                    Status = device.Status.ToString(),
                    LastMeasurement = device.LastMeasurement
                });
            }

            return result;
        }

        public async Task<ExecutionResult> EditDeviceAsync(CreateDeviceReq req, int deviceId)
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
                if (!CheckIfImage(req.ImageFile))
                {
                    result.Message = "Invalid image file type. Only JPEG and PNG are allowed.";
                    return result;
                }

                if (serialChanged && Directory.Exists(oldFolder))
                {
                    device.Image = await UploadImage(device, req);
                    Directory.Delete(oldFolder, true);
                }
                else
                {
                    device.Image = await UploadImage(device, req);
                }
            }
            device.SerialNumber = newSerial;
            device.Location = req.Location;

            var isSuccess = await _deviceGeneralRepository.UpdateAsync(device);
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
                GroupId = groupId
            };

            if (req.ImageFile != null && req.ImageFile.Length > 0)
            {
                if (!CheckIfImage(req.ImageFile))
                {
                    result.Message = "Invalid image file type. Only JPEG and PNG are allowed.";
                    return result;
                }
                device.Image = await UploadImage(null, req);
            }

            var historicalDevice = await _deviceRepository.GetLatestHistoricalRecord(req.SerialNumber);

            if (historicalDevice != null)
            {
                device.Status = historicalDevice.Status;
                device.LastMeasurement = historicalDevice.LastMeasurement;
            }

            var isSuccess = await _deviceGeneralRepository.AddAsync(device);
            result.Success = isSuccess;
            return result;
        }

        public async Task<MeasurementResponse> GetDeviceMeasurementAsync(int deviceId, string parameterType)
        {
            var devicecSerialNumber = await _deviceRepository.GetDeviceSerialNumberAsync(deviceId);

            var result = new MeasurementResponse()
            {
                Parameter = parameterType,
            };

            if (string.IsNullOrEmpty(devicecSerialNumber))
                return result;

            var deviceMeasurements = await _firebaseRepository.GetDeviceMeasurementAsync(devicecSerialNumber);

            foreach (var measurement in deviceMeasurements)
            {
                var deviceParameter = measurement.Parameters.FirstOrDefault(x => x.ContainsKey(parameterType));

                if (deviceParameter != null && double.TryParse(deviceParameter[parameterType].ToString(), out var parsedValue))
                {
                    result.Measurements[DateTimeOffset.FromUnixTimeSeconds(measurement.Timestamp)] = parsedValue;
                }
            }
            result.Measurements = result.Measurements
                                        .OrderByDescending(kv => kv.Key)
                                        .ToDictionary(kv => kv.Key, kv => kv.Value);
            return result;
        }
    }
}
