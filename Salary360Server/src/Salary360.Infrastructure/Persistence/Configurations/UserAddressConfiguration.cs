using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salary360.Domain.Entities;
using Salary360.Infrastructure.Identity;

namespace Salary360.Infrastructure.Persistence.Configurations;

public sealed class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("UserAddresses");
        builder.HasKey(address => address.Id);
        builder.HasIndex(address => address.UserId).IsUnique();

        builder.Property(address => address.PostalCode).HasMaxLength(20).IsRequired();
        builder.Property(address => address.Street).HasMaxLength(200).IsRequired();
        builder.Property(address => address.Number).HasMaxLength(20).IsRequired();
        builder.Property(address => address.Complement).HasMaxLength(100);
        builder.Property(address => address.Neighborhood).HasMaxLength(100).IsRequired();
        builder.Property(address => address.City).HasMaxLength(100).IsRequired();
        builder.Property(address => address.State).HasMaxLength(100).IsRequired();
        builder.Property(address => address.Country).HasMaxLength(2).IsRequired();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<UserAddress>(address => address.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}