using System.ComponentModel.DataAnnotations;

namespace ATRIUM.Web.ViewModels.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class ValidBirthDateAttribute : ValidationAttribute
{
    private static readonly DateTime MinimumDate = new(1900, 1, 1);

    public ValidBirthDateAttribute()
    {
        ErrorMessage = "Ingresa una fecha de nacimiento válida.";
    }

    public override bool IsValid(object? value)
    {
        if (value is not DateTime date)
            return false;

        var normalizedDate = date.Date;
        return normalizedDate >= MinimumDate && normalizedDate <= DateTime.Today;
    }
}
