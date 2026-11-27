namespace Salary360.Application.Common;

public static class AppPermissions
{
    public const string ClaimType = "permission";

    public static class Profile
    {
        public const string View = "Profile.View";
        public const string Update = "Profile.Update";
    }

    public static class Users
    {
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Update = "Users.Update";
        public const string Delete = "Users.Delete";
    }

    public static class Roles
    {
        public const string View = "Roles.View";
        public const string Create = "Roles.Create";
        public const string Update = "Roles.Update";
        public const string Delete = "Roles.Delete";
    }

    public static class Salaries
    {
        public const string View = "Salaries.View";
        public const string Create = "Salaries.Create";
        public const string Update = "Salaries.Update";
        public const string Delete = "Salaries.Delete";
    }

    public static class Expenses
    {
        public const string View = "Expenses.View";
        public const string Create = "Expenses.Create";
        public const string Update = "Expenses.Update";
        public const string Delete = "Expenses.Delete";
    }

    public static IReadOnlyList<string> All { get; } =
    [
        Profile.View, Profile.Update,
        Users.View, Users.Create, Users.Update, Users.Delete,
        Roles.View, Roles.Create, Roles.Update, Roles.Delete,
        Salaries.View, Salaries.Create, Salaries.Update, Salaries.Delete,
        Expenses.View, Expenses.Create, Expenses.Update, Expenses.Delete
    ];

    private static readonly IReadOnlyList<string> UserPermissions =
    [
        Profile.View, Profile.Update,
        Salaries.View, Salaries.Create, Salaries.Update, Salaries.Delete,
        Expenses.View, Expenses.Create, Expenses.Update, Expenses.Delete
    ];

    public static IReadOnlyDictionary<string, IReadOnlyList<string>> ByRole { get; } =
        new Dictionary<string, IReadOnlyList<string>>
        {
            [AppRoles.Admin] = All,
            [AppRoles.User] = UserPermissions
        };
}
