using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salary360.Domain.Entities;

namespace Salary360.Infrastructure.Persistence.Configurations;

public sealed class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.ToTable("ExpenseCategories");
        builder.HasKey(category => category.Id);
        builder.HasIndex(category => category.UserId);
        builder.HasIndex(category => new { category.UserId, category.SystemKey });

        builder.Property(category => category.Name).HasMaxLength(ExpenseCategory.MaxNameLength);
        builder.Property(category => category.SystemKey).HasMaxLength(32);
        builder.Property(category => category.ColorKey).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(category => category.IconKey).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(category => category.CreatedAtUtc).IsRequired();
        builder.Property(category => category.UpdatedAtUtc).IsRequired();
        builder.Ignore(category => category.DomainEvents);
    }
}
