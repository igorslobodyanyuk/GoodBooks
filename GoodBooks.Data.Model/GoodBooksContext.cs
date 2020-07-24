using GoodBooks.Data.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace GoodBooks.Data.Model
{
    public class GoodBooksContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Review> Reviews { get; set; }

        public GoodBooksContext()
        {
        }

        public GoodBooksContext(DbContextOptions<GoodBooksContext> options)
            : base(options)
        { 
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Reviews)
                .WithOne(b => b.Book)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
