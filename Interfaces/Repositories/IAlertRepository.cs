using Models.SqlEntities;

namespace Interfaces.Repositories
{
    public interface IAlertRepository
    {
        Task<IEnumerable<Alert>> GetDeviceAlertRules(int deviceId);
    }
}
