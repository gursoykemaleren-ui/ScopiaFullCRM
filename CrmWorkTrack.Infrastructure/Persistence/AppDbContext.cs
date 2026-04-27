using Microsoft.EntityFrameworkCore;
using CrmWorkTrack.Domain.Entities;

namespace CrmWorkTrack.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobComment> JobComments => Set<JobComment>();
    public DbSet<JobActivity> JobActivities => Set<JobActivity>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Attachment> Attachments { get; set; }

    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<CustomerInteraction> CustomerInteractions { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // USER
        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("Users");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("UserId");

            b.Property(x => x.Email)
                .HasMaxLength(256)
                .IsRequired();

            b.HasIndex(x => x.Email)
                .IsUnique()
                .HasDatabaseName("UX_Users_Email");

            b.Property(x => x.UserName)
                .HasMaxLength(100)
                .IsRequired();

            b.Property(x => x.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            b.Property(x => x.UniqueKey)
                .HasMaxLength(100);

            b.HasIndex(x => x.UniqueKey)
                .IsUnique()
                .HasDatabaseName("UX_Users_UniqueKey");

            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt);
        });

        // ROLE
        modelBuilder.Entity<Role>(b =>
        {
            b.ToTable("Roles");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("RoleId");

            b.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            b.Property(x => x.Description)
                .HasMaxLength(300);

            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt);

            b.HasMany(x => x.UserRoles)
                .WithOne(x => x.Role)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(x => x.RolePermissions)
                .WithOne(x => x.Role)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PERMISSION
        modelBuilder.Entity<Permission>(b =>
        {
            b.ToTable("Permissions");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("PermissionId");

            b.Property(x => x.Code)
                .HasMaxLength(100)
                .IsRequired();

            b.HasIndex(x => x.Code).IsUnique();

            b.Property(x => x.Description)
                .HasMaxLength(500);

            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt);

            b.HasMany(x => x.RolePermissions)
                .WithOne(x => x.Permission)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // USER ROLE
        modelBuilder.Entity<UserRole>(b =>
        {
            b.ToTable("UserRoles");

            b.HasKey(x => new { x.UserId, x.RoleId });

            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.UpdatedAt);

            b.HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ROLE PERMISSION
        modelBuilder.Entity<RolePermission>(b =>
        {
            b.ToTable("RolePermissions");

            b.HasKey(x => new { x.RoleId, x.PermissionId });

            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.UpdatedAt);

            b.HasOne(x => x.Role)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Permission)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CUSTOMER
        modelBuilder.Entity<Customer>(b =>
        {
            b.ToTable("Customers");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("CustomerId");

            b.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
            b.Property(x => x.ContactName).HasMaxLength(150);
            b.Property(x => x.Email).HasMaxLength(256);
            b.Property(x => x.Phone).HasMaxLength(30);

            b.Property(x => x.Address).HasMaxLength(300);
            b.Property(x => x.City).HasMaxLength(100);
            b.Property(x => x.Notes).HasMaxLength(1000);

            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt);

            // Audit
            b.Property(x => x.UpdatedByUserId);

            // Soft Delete
            b.Property(x => x.IsDeleted).IsRequired();
            b.Property(x => x.DeletedAt);
            b.Property(x => x.DeletedByUserId);

            b.HasQueryFilter(x => !x.IsDeleted);
        });

        // JOB
        modelBuilder.Entity<Job>(b =>
        {
            b.ToTable("Jobs");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("JobId");

            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.IsCompleted).HasMaxLength(50).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.Priority).HasMaxLength(50).IsRequired();
            b.Property(x => x.DueDate);

            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.UpdatedAt);

            // Audit
            b.Property(x => x.UpdatedByUserId);

            // Soft Delete
            b.Property(x => x.IsDeleted).IsRequired();
            b.Property(x => x.DeletedAt);
            b.Property(x => x.DeletedByUserId);

            b.HasIndex(x => x.IsCompleted);
            b.HasIndex(x => x.CustomerId);
            b.HasIndex(x => x.CreatedByUserId);
            b.HasIndex(x => x.AssignedToUserId);

            b.HasOne(x => x.Customer)
                .WithMany(x => x.Jobs)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CreatedJobs)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.AssignedToUser)
                .WithMany(x => x.AssignedJobs)
                .HasForeignKey(x => x.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasQueryFilter(x => !x.IsDeleted);
        });

        // JOB COMMENT
        modelBuilder.Entity<JobComment>(b =>
        {
            b.ToTable("JobComments");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("JobCommentId");

            b.Property(x => x.JobId).HasColumnName("JobId").IsRequired();
            b.Property(x => x.CreatedByUserId).HasColumnName("CreatedByUserId").IsRequired();

            b.Property(x => x.Text).HasColumnName("Text").HasMaxLength(4000).IsRequired();
            b.Property(x => x.IsActive).HasColumnName("IsActive").IsRequired();

            b.Property(x => x.CreatedAt).HasColumnName("CreatedAt").IsRequired();
            b.Property(x => x.UpdatedAt).HasColumnName("UpdatedAt");

            // Audit
            b.Property(x => x.UpdatedByUserId).HasColumnName("UpdatedByUserId");

            // Soft Delete
            b.Property(x => x.IsDeleted).HasColumnName("IsDeleted").IsRequired();
            b.Property(x => x.DeletedAt).HasColumnName("DeletedAt");
            b.Property(x => x.DeletedByUserId).HasColumnName("DeletedByUserId");

            b.HasIndex(x => x.JobId).HasDatabaseName("IX_JobComments_JobId");
            b.HasIndex(x => x.CreatedByUserId).HasDatabaseName("IX_JobComments_CreatedByUserId");

            b.HasQueryFilter(x => !x.IsDeleted);
        });

        // JOB ACTIVITIES
        modelBuilder.Entity<JobActivity>(b =>
        {
            b.ToTable("JobActivities");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("JobActivityId");

            b.Property(x => x.JobId).HasColumnName("JobId").IsRequired();

            b.Property(x => x.Type).HasColumnName("Type").HasMaxLength(100).IsRequired();
            b.Property(x => x.Message).HasColumnName("Message").HasMaxLength(1000);
            b.Property(x => x.MetaJson).HasColumnName("MetaJson").HasMaxLength(4000);

            b.Property(x => x.PerformedByUserId).HasColumnName("PerformedByUserId");

            b.Property(x => x.CreatedAt).HasColumnName("CreatedAt").IsRequired();

            b.HasIndex(x => x.JobId).HasDatabaseName("IX_JobActivities_JobId");
            b.HasIndex(x => x.PerformedByUserId).HasDatabaseName("IX_JobActivities_PerformedByUserId");
        });
        modelBuilder.Entity<CustomerContact>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Title)
                .HasMaxLength(100);

            entity.Property(x => x.Email)
                .HasMaxLength(200);

            entity.Property(x => x.Phone)
                .HasMaxLength(50);

            entity.Property(x => x.MobilePhone)
                .HasMaxLength(50);

            entity.Property(x => x.Notes)
                .HasMaxLength(2000);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Contacts)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    
    modelBuilder.Entity<RefreshToken>(b =>
        {
            b.ToTable("RefreshTokens");

            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("RefreshTokenId");

            b.Property(x => x.TokenHash).HasMaxLength(256).IsRequired();
            b.Property(x => x.TokenSalt).HasMaxLength(256).IsRequired();

            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.ExpiresAt).IsRequired();

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.UserId, x.ExpiresAt });
        });

        // TICKET
        modelBuilder.Entity<Ticket>()
    .HasOne(x => x.Customer)
    .WithMany()
    .HasForeignKey(x => x.CustomerId)
    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Ticket>()
            .HasOne(x => x.AssignedToUser)
            .WithMany()
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CustomerInteraction>()
            .HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerInteraction>()
            .HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}