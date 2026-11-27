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

    public static IReadOnlyList<string> All { get; } =
    [
        Profile.View, Profile.Update,
        Users.View, Users.Create, Users.Update, Users.Delete,
        Roles.View, Roles.Create, Roles.Update, Roles.Delete
    ];

    public static IReadOnlyDictionary<string, IReadOnlyList<string>> ByRole { get; } =
        new Dictionary<string, IReadOnlyList<string>>
        {
            [AppRoles.Admin] = All,
            [AppRoles.User] = [Profile.View, Profile.Update]
        };
}
