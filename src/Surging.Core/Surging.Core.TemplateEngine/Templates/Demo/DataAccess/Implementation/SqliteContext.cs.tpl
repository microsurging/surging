using Microsoft.EntityFrameworkCore;
using {{ prefix }}.DataAccess.{{ project.name }};

namespace {{ prefix }}.DataAccess.{{ project.name }}.Implementation
{
    public class SqliteContext : DataContext
    {
        private readonly string? _connstring = "Data Source=KayakData.db;";
        public SqliteContext() { }

        public SqliteContext(string? connstring)
        {
            _connstring = connstring;
        }
       public SqliteContext(DbContextOptions<DataContext> options):base(options)
        {
            _connstring = null;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_connstring!=null)
            {
                optionsBuilder.UseSqlite(_connstring);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public override async Task InitializeAsync()
        {
            await Database.MigrateAsync();
        }
    }


}
