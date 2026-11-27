using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Salary360.Domain.Entities;

namespace Salary360.Infrastructure.Persistence.Configurations;

public sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");
        builder.HasKey(expense => expense.Id);
        builder.HasIndex(expense => new { expense.UserId, expense.Year, expense.Month });
        builder.HasIndex(expense => new { expense.UserId, expense.CategoryId });
        builder.HasIndex(expense => expense.SeriesId);

        builder.Property(expense => expense.Name).HasMaxLength(Expense.MaxNameLength).IsRequired();
        builder.Property(expense => expense.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(expense => expense.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(expense => expense.Notes).HasMaxLength(Expense.MaxNotesLength);
        builder.Property(expense => expense.CreatedAtUtc).IsRequired();
        builder.Property(expense => expense.UpdatedAtUtc).IsRequired();
        builder.Ignore(expense => expense.DomainEvents);

        builder.HasOne<ExpenseCategory>()
            .WithMany()
            .HasForeignKey(expense => expense.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
