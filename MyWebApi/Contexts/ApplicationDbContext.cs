using Microsoft.EntityFrameworkCore;
using MyWebApi.Entities;

namespace MyWebApi.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Person> People { get; set; }
    }
}
