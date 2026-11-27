using Salary360.Domain.Enums;

namespace Salary360.Domain.ExpenseCategories;

public static class DefaultExpenseCategories
{
    public static IReadOnlyList<(string SystemKey, CategoryColor Color, CategoryIcon Icon)> All { get; } =
    [
        ("housing", CategoryColor.Blue, CategoryIcon.Home),
        ("food", CategoryColor.Orange, CategoryIcon.ShoppingBag),
        ("transport", CategoryColor.Amber, CategoryIcon.Truck),
        ("health", CategoryColor.Red, CategoryIcon.Heart),
        ("education", CategoryColor.Violet, CategoryIcon.AcademicCap),
        ("leisure", CategoryColor.Pink, CategoryIcon.Sparkles),
        ("subscriptions", CategoryColor.Teal, CategoryIcon.CreditCard),
        ("taxes", CategoryColor.Slate, CategoryIcon.ReceiptPercent),
        ("debts", CategoryColor.Green, CategoryIcon.Banknotes),
        ("other", CategoryColor.Slate, CategoryIcon.EllipsisHorizontal)
    ];
}
