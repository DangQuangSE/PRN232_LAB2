using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PRN232.LMSSystem.Services.Helpers;

/// <summary>
/// Custom validation attribute to validate FPT University Student Code.
/// Format: 2 uppercase letters representing campus/major (e.g., SE, CE, HE, IA, GD) followed by exactly 5 or 6 digits.
/// Example: SE19886, CE18793.
/// </summary>
public class FptuStudentCodeAttribute : ValidationAttribute
{
    private static readonly Regex FptuCodeRegex = new(@"^[A-Z]{2}\d{5,6}$", RegexOptions.Compiled);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return new ValidationResult("Student code is required.");
        }

        var code = value.ToString()!.Trim().ToUpper();

        if (!FptuCodeRegex.IsMatch(code))
        {
            return new ValidationResult("Student code must match FPTU style (e.g. SE19886, CE18793).");
        }

        // Custom validation check: first 2 characters must be one of the known FPTU prefixes
        var prefix = code.Substring(0, 2);
        string[] validPrefixes = { "SE", "CE", "IA", "GD", "HE", "MC", "DE", "QE", "SA" };
        
        if (!validPrefixes.Contains(prefix))
        {
            return new ValidationResult($"Invalid FPTU student major prefix '{prefix}'. Must be one of: {string.Join(", ", validPrefixes)}.");
        }

        return ValidationResult.Success;
    }
}
