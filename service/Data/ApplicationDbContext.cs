using Microsoft.EntityFrameworkCore;
using WhatsAppCampaignManager.Models;

namespace WhatsAppCampaignManager.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<AppInstance> AppInstances { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppUserInstance> AppUserInstances { get; set; }
        public DbSet<AppGroup> AppGroups { get; set; }
        public DbSet<AppMessage> AppMessages { get; set; }
        public DbSet<AppMessageGroup> AppMessageGroups { get; set; }
        public DbSet<AppJob> AppJobs { get; set; }
        public DbSet<AppJobLog> AppJobLogs { get; set; }
        public DbSet<AppSentMessage> AppSentMessages { get; set; }
        public DbSet<AppAnalys> AppAnalytics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // AppUserInstance - Composite relationships
            modelBuilder.Entity<AppUserInstance>()
                .HasOne(ui => ui.User)
                .WithMany(u => u.UserInstances)
                .HasForeignKey(ui => ui.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppUserInstance>()
                .HasOne(ui => ui.Instance)
                .WithMany(i => i.UserInstances)
                .HasForeignKey(ui => ui.InstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            // AppMessageGroup - Composite relationships
            modelBuilder.Entity<AppMessageGroup>()
                .HasOne(mg => mg.Message)
                .WithMany(m => m.MessageGroups)
                .HasForeignKey(mg => mg.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppMessageGroup>()
                .HasOne(mg => mg.Group)
                .WithMany(g => g.MessageGroups)
                .HasForeignKey(mg => mg.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // AppMessage relationships
            modelBuilder.Entity<AppMessage>()
                .HasOne(m => m.CreatedByUser)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppMessage>()
                .HasOne(m => m.Instance)
                .WithMany()
                .HasForeignKey(m => m.InstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            // AppJob relationships
            modelBuilder.Entity<AppJob>()
                .HasOne(j => j.Message)
                .WithMany(m => m.Jobs)
                .HasForeignKey(j => j.MessageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppJob>()
                .HasOne(j => j.Instance)
                .WithMany(i => i.Jobs)
                .HasForeignKey(j => j.InstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppJob>()
                .HasOne(j => j.CreatedByUser)
                .WithMany(u => u.Jobs)
                .HasForeignKey(j => j.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // AppJobLog relationships
            modelBuilder.Entity<AppJobLog>()
                .HasOne(jl => jl.Job)
                .WithMany(j => j.JobLogs)
                .HasForeignKey(jl => jl.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // AppSentMessage relationships
            modelBuilder.Entity<AppSentMessage>()
                .HasOne(sm => sm.Job)
                .WithMany(j => j.SentMessages)
                .HasForeignKey(sm => sm.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            // AppAnalys relationships
            modelBuilder.Entity<AppAnalys>()
                .HasOne(a => a.Group)
                .WithMany(g => g.Analytics)
                .HasForeignKey(a => a.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            modelBuilder.Entity<AppInstance>()
                .HasIndex(i => i.WhatsAppNumber)
                .IsUnique();

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<AppGroup>()
                .HasIndex(g => g.GroupId)
                .IsUnique();

            modelBuilder.Entity<AppGroup>()
                .HasOne(j => j.Instance)
                .WithMany(i => i.Groups)
                .HasForeignKey(j => j.InstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AppAnalys>()
                .HasIndex(a => new { a.GroupId, a.UserPhoneNumber })
                .IsUnique();

            // Seed data
            modelBuilder.Entity<AppUser>().HasData(
                new AppUser
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@whatsappcampaign.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("654321"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed data
            modelBuilder.Entity<AppUser>().HasData(
                new AppUser
                {
                    Id = 2,
                    Username = "user1",
                    Email = "user1@whatsappcampaign.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Role = "Member",
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
