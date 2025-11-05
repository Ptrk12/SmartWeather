using Interfaces.Managers;
using Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Models.requests;
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

        public async Task<bool> AddDeviceAsync(CreateDeviceReq req, int groupId)
        {
            string? imagePath = null;

            if (req.ImageFile != null && req.ImageFile.Length > 0)
            {
                var rootPath = _configuration.GetSection("Images").GetValue<string>("StoragePath");

                var deviceFolder = Path.Combine(rootPath, req.SerialNumber);

                if (!Directory.Exists(deviceFolder))
                    Directory.CreateDirectory(deviceFolder);

                var extension = Path.GetExtension(req.ImageFile.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var fullPath = Path.Combine(deviceFolder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await req.ImageFile.CopyToAsync(stream);
                }

                imagePath = Path.Combine(req.SerialNumber, fileName).Replace("\\", "/");
            }

            var device = new Device()
            {
                SerialNumber = req.SerialNumber,
                Location = req.Location,
                Image = imagePath,
                GroupId = groupId
            };

            return await _deviceRepository.AddAsync(device);
        }
    }
}
