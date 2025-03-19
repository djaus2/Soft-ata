using Microsoft.EntityFrameworkCore;

namespace SoftataWebAPI.Data.Db
{
    public class SoftataDbContext : DbContext
    {
        public SoftataDbContext(DbContextOptions<SoftataDbContext> options) : base(options)
        {
        }
        public DbSet<DType> dTypes { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<GenericCommand> GenericCommands { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=dictionary.db");
            }
        }
    }
}

