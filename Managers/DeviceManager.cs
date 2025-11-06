using Interfaces.Managers;
using Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Models.requests;
using Models.responses;
using Models.SqlEntities;

namespace Managers
{
    public class DeviceManager : IDeviceManager
    {
        private readonly IGenericCrudRepository<Device> _deviceRepository;
        private readonly IGenericCrudRepository<Group> _groupRepository;
        private readonly IConfiguration _configuration;

        public DeviceManager(
            IGenericCrudRepository<Device> deviceRepository,
            IGenericCrudRepository<Group> groupRepository,
            IConfiguration configuration)
        {
            _deviceRepository = deviceRepository;
            _groupRepository = groupRepository;
            _configuration = configuration;
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

        public async Task<ExecutionResult> EditDeviceAsync(CreateDeviceReq req, int deviceId, int groupId)
        {
            var result = new ExecutionResult();
            var device = await _deviceRepository.GetByIdAsync(deviceId);
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

            var isSuccess = await _deviceRepository.UpdateAsync(device);
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

            var isSuccess = await _deviceRepository.AddAsync(device);
            result.Success = isSuccess;
            return result;
        }
    }
}
