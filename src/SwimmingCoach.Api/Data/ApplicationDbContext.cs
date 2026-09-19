using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SwimmingCoach.Api.Identity;
using SwimmingCoach.Api.Profiles;

namespace SwimmingCoach.Api.Data;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<AthleteProfile> AthleteProfiles => Set<AthleteProfile>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AthleteProfile>(profile =>
        {
            profile.ToTable("AthleteProfiles");
            profile.HasKey(value => value.Id);

            profile.Property(value => value.UserId)
                .HasMaxLength(255)
                .IsRequired();
            profile.HasIndex(value => value.UserId).IsUnique();

            profile.Property(value => value.TimeZoneId)
                .HasMaxLength(100)
                .IsRequired();
            profile.Property(value => value.PreferredUnits)
                .HasConversion<string>()
                .HasMaxLength(20);
            profile.Property(value => value.PoolLength)
                .HasPrecision(5, 2);
            profile.Property(value => value.PoolLengthUnit)
                .HasConversion<string>()
                .HasMaxLength(20);
            profile.Property(value => value.SwimmingExperience)
                .HasConversion<string>()
                .HasMaxLength(20);
            profile.Property(value => value.RunningExperience)
                .HasConversion<string>()
                .HasMaxLength(20);

            profile.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<AthleteProfile>(value => value.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
