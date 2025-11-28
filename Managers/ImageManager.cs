using Interfaces.Managers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Models.requests;
using Models.SqlEntities;

namespace Managers
{
    public class ImageManager : IImageManager
    {
        private readonly IConfiguration _configuration;

        public ImageManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public bool CheckIfImage(IFormFile? file)
        {
            var allowedMimeTypes = new[] { "image/jpeg", "image/png" };

            if (file != null && !allowedMimeTypes.Contains(file.ContentType))
            {
                return false;
            }
            return true;
        }

        public async Task<string> UploadImage(Device? device, CreateDeviceReq req)
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

        public void DeleteImageAsync(string deviceSerialNumber)
        {
            var rootPath = _configuration.GetSection("Images").GetValue<string>("StoragePath");
            var deviceFolder = Path.Combine(rootPath, deviceSerialNumber);
            if (Directory.Exists(deviceFolder))
            {
                Directory.Delete(deviceFolder, true);
            }
        }
    }
}
