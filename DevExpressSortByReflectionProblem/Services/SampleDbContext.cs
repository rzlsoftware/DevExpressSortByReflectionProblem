using DevExpressSortByReflectionProblem.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevExpressSortByReflectionProblem.Services
{
    public class SampleDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer("Server=localhost;Database=DevExpressSortByReflectionProblem;Trusted_Connection=True");

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
    }
}
