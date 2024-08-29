using BMS_API.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityManager.Data;


public class ApplicationDbContext : IdentityDbContext<SystemUser>
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<SystemUser> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Business> Businesses { get; set; }
    public DbSet<UserBusinessRole> UserBusinessRoles { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserProfile>()
            .HasOne(up => up.User)
            .WithOne(u => u.UserProfile)
            .HasForeignKey<UserProfile>(up => up.UserId);

        modelBuilder.Entity<UserProfile>()
                .HasMany(up => up.AuditLogs)
                .WithOne(al => al.UserProfile)
                .HasForeignKey(al => al.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserProfile>()
            .HasOne(up => up.PermanentAddress)
            .WithMany()
            .HasForeignKey(up => up.PermanentAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserProfile>()
            .HasOne(up => up.TemporaryAddress)
            .WithMany()
            .HasForeignKey(up => up.TemporaryAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        // UserBusinessRole relationships
        modelBuilder.Entity<UserBusinessRole>()
            .HasOne(ub => ub.User)
            .WithMany(u => u.UserBusinessRoles)
            .HasForeignKey(ub => ub.UserId);

        modelBuilder.Entity<UserBusinessRole>()
            .HasOne(ub => ub.Business)
            .WithMany(b => b.UserBusinessRoles)
            .HasForeignKey(ub => ub.BusinessId);

        // Store enum as string in the database
        modelBuilder.Entity<UserBusinessRole>()
            .Property(ub => ub.Role)
            .HasConversion<string>();

        // Ensure that each business has at least one owner role
        // Do it on controller
        // modelBuilder.Entity<UserBusinessRole>()
        //     .HasIndex(ub => new { ub.BusinessId, ub.Role })
        //     .IsUnique()
        //     .HasFilter($"[{nameof(UserBusinessRole.Role)}] = '{BusinessRole.SuperOwner}'");

    }
}