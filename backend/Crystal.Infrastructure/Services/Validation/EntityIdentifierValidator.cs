using Crystal.Core.Constants;

namespace Crystal.Infrastructure.Services.Validation;

public static class EntityIdentifierValidator
{
    public static void EnsureValid(int p_id)
    {
        if (p_id <= 0)
        {
            throw new ArgumentException(ErrorMessages.InvalidIdentifier);
        }
    }

    public static void EnsureValidEmployeeProfileId(int p_employeeProfileId)
    {
        if (p_employeeProfileId <= 0)
        {
            throw new ArgumentException(ErrorMessages.EmployeeProfile.InvalidIdentifier);
        }
    }
}
