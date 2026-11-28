using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salary360.Infrastructure.Identity;

namespace Salary360.Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(user => user.ProfilePictureUrl).HasMaxLength(2048);
    }
}