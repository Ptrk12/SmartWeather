
using Models.SqlEntities;
using Repositories.SqlContext;

namespace Repositories
{
    public class DeviceRepository : GenericCrudRepository<Device>
    {
        private readonly SqlDbContext _context;

        public DeviceRepository(SqlDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
