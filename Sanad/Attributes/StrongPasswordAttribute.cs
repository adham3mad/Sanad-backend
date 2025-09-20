using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class StrongPasswordAttribute : ValidationAttribute
{
    private static readonly Regex HasUpper = new Regex(@"[A-Z]+", RegexOptions.Compiled);
    private static readonly Regex HasLower = new Regex(@"[a-z]+", RegexOptions.Compiled);
    private static readonly Regex HasDigit = new Regex(@"\d+", RegexOptions.Compiled);
    private static readonly Regex HasSpecial = new Regex(@"[\W_]+", RegexOptions.Compiled);

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var password = value as string;

        if (string.IsNullOrWhiteSpace(password))
            return ValidationResult.Success;

        if (!HasUpper.IsMatch(password))
            return new ValidationResult("Password must contain at least one uppercase letter.");

        if (!HasLower.IsMatch(password))
            return new ValidationResult("Password must contain at least one lowercase letter.");

        if (!HasDigit.IsMatch(password))
            return new ValidationResult("Password must contain at least one digit.");

        if (!HasSpecial.IsMatch(password))
            return new ValidationResult("Password must contain at least one special character.");

        return ValidationResult.Success;
    }
}
