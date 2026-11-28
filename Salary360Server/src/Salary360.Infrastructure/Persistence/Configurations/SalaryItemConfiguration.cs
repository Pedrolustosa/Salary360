using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salary360.Domain.Entities;

namespace Salary360.Infrastructure.Persistence.Configurations;

public sealed class SalaryItemConfiguration : IEntityTypeConfiguration<SalaryItem>
{
    public void Configure(EntityTypeBuilder<SalaryItem> builder)
    {
        builder.ToTable("SalaryItems");
        builder.HasKey(item => item.Id);

        builder.Property(item => item.Name).HasMaxLength(SalaryRecord.MaxItemNameLength).IsRequired();
        builder.Property(item => item.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.Order).IsRequired();
        builder.Property(item => item.Type)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
    }
}