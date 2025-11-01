
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models.SqlEntities;
using System.Reflection.Emit;

namespace Repositories.SqlContext
{
    public class SqlDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<AlertLog> AlertLogs { get; set; }
        public DbSet<GroupMembership> GroupMemberships { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<SensorMetric> SensorMetrics { get; set; }
        public DbSet<Group> Groups { get; set; }

        public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<GroupMembership>()
                .Property(g => g.Role)
                .HasConversion<string>();

            builder.Entity<Device>()
                .Property(g => g.Status)
                .HasConversion<string>();


            builder.Entity<GroupMembership>()
                .HasKey(x => new { x.GroupId, x.ApplicationUserId });

            builder.Entity<GroupMembership>()
                .HasOne(x => x.Group)
                .WithMany(x=>x.Memberships)
                .HasForeignKey(x => x.GroupId);

            builder.Entity<GroupMembership>()
                .HasOne(x => x.ApplicationUser)
                .WithMany(x => x.GroupMemberships)
                .HasForeignKey(x => x.ApplicationUserId);

            builder.Entity<Device>()
                .HasOne(x => x.Group)
                .WithMany(x=>x.Devices)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SensorMetric>()
                .HasOne(x=>x.Device)
                .WithMany(x=>x.Metrics)
                .HasForeignKey(x=>x.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Alert>()
                .HasOne(x=>x.SensorMetric)
                .WithMany(x=>x.Alerts)
                .HasForeignKey(x=>x.SensorMetricId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AlertLog>()
                .HasOne(al => al.Alert)
                .WithMany(a => a.AlertLogs) 
                .HasForeignKey(al => al.AlertId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
