using System.ComponentModel.DataAnnotations;

namespace BMS_API.Models
{
    public class CustomPasswordValidationAttribute : ValidationAttribute
    {
        private readonly int _minLength;
        private readonly int _requiredUniqueChars;

        public CustomPasswordValidationAttribute(int minLength = 8, int requiredUniqueChars = 1)
        {
            _minLength = minLength;
            _requiredUniqueChars = requiredUniqueChars;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;

            if (string.IsNullOrEmpty(password))
            {
                return new ValidationResult("Password is required.");
            }

            if (password.Length < _minLength)
            {
                return new ValidationResult($"Password must be at least {_minLength} characters long.");
            }

            if (!password.Any(char.IsUpper))
            {
                return new ValidationResult("Password must contain at least one uppercase letter.");
            }

            if (!password.Any(char.IsLower))
            {
                return new ValidationResult("Password must contain at least one lowercase letter.");
            }

            if (!password.Any(char.IsDigit))
            {
                return new ValidationResult("Password must contain at least one digit.");
            }

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                return new ValidationResult("Password must contain at least one non-alphanumeric character.");
            }

            if (password.Distinct().Count() < _requiredUniqueChars)
            {
                return new ValidationResult($"Password must contain at least {_requiredUniqueChars} unique characters.");
            }

            return ValidationResult.Success;
        }
    }
}
