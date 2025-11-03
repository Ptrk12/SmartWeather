using Interfaces.Managers;
using Interfaces.Repositories;
using Models.requests;
using Models.SqlEntities;

namespace Managers
{
    public class DeviceManager : IDeviceManager
    {
        private readonly IGenericCrudRepository<Device> _deviceRepository;
        private readonly IGenericCrudRepository<Group> _groupRepository;

        public DeviceManager(
            IGenericCrudRepository<Device> deviceRepository,
            IGenericCrudRepository<Group> groupRepository)
        {
            _deviceRepository = deviceRepository;
            _groupRepository = groupRepository;
        }

        public async Task<bool> AddDeviceAsync(CreateDeviceReq req, int groupId)
        {
            //add image path...
            var device = new Device()
            {
                SerialNumber = req.SerialNumber,
                Location = req.Location,
                Image = req.Image,
                GroupId = groupId
            };

            return await _deviceRepository.AddAsync(device);
        }
    }
}
