using Microsoft.EntityFrameworkCore;
using SIBA.ComplaintSystem.Models;

namespace SIBA.ComplaintSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ComplaintMessage> ComplaintMessages { get; set; }
        public DbSet<ComplaintArchive> ComplaintArchives { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "senior@admin.siba.edu.pk",
                    Password = "Admin@123", // In production, this should be hashed!
                    Role = "Admin",
                    CreatedAt = new DateTime(2026, 4, 1) // Fixed date for consistency
                }
            );

            // Configure Relationship
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<ComplaintMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
