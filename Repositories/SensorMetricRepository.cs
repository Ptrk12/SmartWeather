using Interfaces.Repositories;
using Models.SqlEntities;
using Repositories.SqlContext;

namespace Repositories
{
    public class SensorMetricRepository : GenericCrudRepository<SensorMetric>, ISensorMetricRepository
    {
        private readonly SqlDbContext _context;

        public SensorMetricRepository(SqlDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
