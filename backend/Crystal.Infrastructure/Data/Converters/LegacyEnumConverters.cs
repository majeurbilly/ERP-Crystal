using Crystal.Core.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Crystal.Infrastructure.Data.Converters;

public static class LegacyEnumConverters
{
    public static ValueConverter<ContractType, string> ForContractType => new(
        p_enum => p_enum.ToString(),
        p_value => ParseContractType(p_value));

    public static ValueConverter<WageType, string> ForWageType => new(
        p_enum => p_enum.ToString(),
        p_value => ParseWageType(p_value));

    private static ContractType ParseContractType(string p_value) => p_value switch
    {
        "CDI" => ContractType.FullTime,
        "CDD" => ContractType.PartTime,
        "Stage" => ContractType.Internship,
        "Freelance" => ContractType.SelfEmployed,
        nameof(ContractType.FullTime) => ContractType.FullTime,
        nameof(ContractType.PartTime) => ContractType.PartTime,
        nameof(ContractType.Internship) => ContractType.Internship,
        nameof(ContractType.SelfEmployed) => ContractType.SelfEmployed,
        _ => Enum.TryParse(p_value, out ContractType parsed) ? parsed : ContractType.FullTime,
    };

    private static WageType ParseWageType(string p_value) => p_value switch
    {
        "Hourly" => WageType.Monthly,
        "Annual" => WageType.Fixed,
        nameof(WageType.Monthly) => WageType.Monthly,
        nameof(WageType.Fixed) => WageType.Fixed,
        _ => Enum.TryParse(p_value, out WageType parsed) ? parsed : WageType.Monthly,
    };
}
