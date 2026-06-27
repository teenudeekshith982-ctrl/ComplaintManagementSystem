using ComplaintManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagementSystem.Contexts;

public class ComplaintManagementSystemContext : DbContext
{
    public ComplaintManagementSystemContext(DbContextOptions<ComplaintManagementSystemContext> options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ComplaintPriority>()
            .HasOne(c => c.SLA)
            .WithOne(s => s.ComplaintPriority)
            .HasForeignKey<SLA>(s => s.PriorityId);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Employee)
            .WithOne(e => e.User)
            .HasForeignKey<Employee>(e => e.UserId);

        modelBuilder.Entity<Complaint>()
            .HasOne(c => c.ComplaintCategory)
            .WithMany(cc => cc.Complaints)
            .HasForeignKey(c => c.CategoryId);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.RelatedComplaint)
            .WithMany()
            .HasForeignKey(n => n.RelatedComplaintId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Phone)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
    

    public DbSet<User> Users { get; set; }

    public DbSet<Complaint> Complaints { get; set; }

    public DbSet<ComplaintCategory> ComplaintCategories { get; set; }

    public DbSet<ComplaintPriority> ComplaintPriorities { get; set; }

    public DbSet<ComplaintStatus> ComplaintStatuses { get; set; }

    public DbSet<ComplaintAttachment> ComplaintAttachments { get; set; }

    public DbSet<Comment> Comments { get; set; }

    public DbSet<ComplaintHistory> ComplaintHistories { get; set; }

    public DbSet<Employee> Employees { get; set; }

    public DbSet<Department> Departments { get; set; }

    public DbSet<EscalatedComplaint> EscalatedComplaints { get; set; }

    public DbSet<EscalatedLevel> EscalatedLevels { get; set; }

    public DbSet<SLA> SLAs { get; set; }

    public DbSet<Notification> Notifications { get; set; }
}