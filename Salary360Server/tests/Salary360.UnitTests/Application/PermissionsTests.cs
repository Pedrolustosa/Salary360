using Salary360.Application.Common;

namespace Salary360.UnitTests.Application;

public sealed class PermissionsTests
{
    [Fact]
    public void Admin_HasEveryPermission()
        => Assert.Equal(AppPermissions.All, AppPermissions.ByRole[AppRoles.Admin]);

    [Fact]
    public void User_HasOnlyProfilePermissions()
        => Assert.Equal([AppPermissions.Profile.View, AppPermissions.Profile.Update], AppPermissions.ByRole[AppRoles.User]);

    [Fact]
    public void Permissions_AreUnique()
        => Assert.Equal(AppPermissions.All.Count, AppPermissions.All.Distinct().Count());
}
