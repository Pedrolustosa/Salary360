using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salary360.Domain.Entities;

namespace Salary360.Infrastructure.Persistence.Configurations;

public sealed class SalaryRecordConfiguration : IEntityTypeConfiguration<SalaryRecord>
{
    public void Configure(EntityTypeBuilder<SalaryRecord> builder)
    {
        builder.ToTable("SalaryRecords");
        builder.HasKey(record => record.Id);
        builder.HasIndex(record => new { record.UserId, record.Year, record.Month }).IsUnique();
        builder.HasIndex(record => record.UserId);

        builder.Property(record => record.GrossAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(record => record.Notes).HasMaxLength(SalaryRecord.MaxNotesLength);
        builder.Property(record => record.CreatedAtUtc).IsRequired();
        builder.Property(record => record.UpdatedAtUtc).IsRequired();

        builder.Ignore(record => record.TotalBenefits);
        builder.Ignore(record => record.TotalDeductions);
        builder.Ignore(record => record.NetAmount);
        builder.Ignore(record => record.DomainEvents);

        builder.HasMany(record => record.Items)
            .WithOne()
            .HasForeignKey(item => item.SalaryRecordId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Navigation(record => record.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}