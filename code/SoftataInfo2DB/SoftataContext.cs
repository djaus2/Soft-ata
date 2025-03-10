using Microsoft.EntityFrameworkCore;

namespace SoftataInfo2DB
{
    public class SoftataContext : DbContext
    {
        public DbSet<DType> dTypes { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<GenericCommand> GenericCommands { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=dictionary.db");
        }
    }
}

