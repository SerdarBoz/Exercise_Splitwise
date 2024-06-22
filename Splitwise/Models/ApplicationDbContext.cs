using Microsoft.EntityFrameworkCore;

namespace Splitwise.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Balance> Balances { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    }
}
