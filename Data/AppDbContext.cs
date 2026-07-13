using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NeighborHelp.Models;

namespace NeighborHelp.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<HelpRequest> HelpRequests { get; set; }
        public DbSet<VolunteerRequest> VolunteerRequests { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AdminLog> AdminLogs { get; set; }
        public DbSet<Location> Locations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<HelpRequest>()
                .HasOne(h => h.User).WithMany()
                .HasForeignKey(h => h.UserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HelpRequest>()
                .HasOne(h => h.Category).WithMany(c => c.HelpRequests)
                .HasForeignKey(h => h.CategoryId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VolunteerRequest>()
                .HasOne(v => v.HelpRequest).WithMany(h => h.VolunteerRequests)
                .HasForeignKey(v => v.RequestId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VolunteerRequest>()
                .HasOne(v => v.User).WithMany()
                .HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VolunteerRequest>()
                .HasIndex(v => new { v.RequestId, v.UserId }).IsUnique();

            modelBuilder.Entity<ChatMessage>()
                .HasOne(c => c.HelpRequest).WithMany()
                .HasForeignKey(c => c.RequestId).OnDelete(DeleteBehavior.Cascade);

            // enums as strings
            modelBuilder.Entity<HelpRequest>()
                .Property(h => h.Status).HasConversion<string>();
            modelBuilder.Entity<VolunteerRequest>()
                .Property(v => v.Status).HasConversion<string>();

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Rater).WithMany()
                .HasForeignKey(r => r.RaterId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.RatedUser).WithMany()
                .HasForeignKey(r => r.RatedUserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.HelpRequest).WithMany()
                .HasForeignKey(r => r.RequestId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(rv => rv.Rating).WithOne(r => r.Review)
                .HasForeignKey<Review>(rv => rv.RatingId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User).WithMany()
                .HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Type).HasConversion<string>();

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });

            modelBuilder.Entity<AdminLog>()
                .HasOne(a => a.Admin).WithMany()
                .HasForeignKey(a => a.AdminId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Groceries & Errands" },
                new Category { CategoryId = 2, Name = "Home Repair" },
                new Category { CategoryId = 3, Name = "Moving & Heavy Lifting" },
                new Category { CategoryId = 4, Name = "Tech Support" },
                new Category { CategoryId = 5, Name = "Tutoring & Mentoring" },
                new Category { CategoryId = 6, Name = "Pet Care" },
                new Category { CategoryId = 7, Name = "Elderly Assistance" },
                new Category { CategoryId = 8, Name = "Other" }
            );
        }
    }
}
