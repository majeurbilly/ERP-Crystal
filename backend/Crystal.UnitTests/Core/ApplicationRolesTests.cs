using Crystal.Core;

namespace Crystal.UnitTests.Core;

public class ApplicationRolesTests
{
    [Fact]
    public void All_Contains_Exactly_The_Four_Expected_Roles_In_Defined_Order()
    {
        string[] expected =
        [
            ApplicationRoles.Admin,
            ApplicationRoles.Gerant,
            ApplicationRoles.Assistant,
            ApplicationRoles.Employee
        ];

        Assert.Equal(expected, ApplicationRoles.All);
    }
}
