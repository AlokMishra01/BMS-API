using System.Text.RegularExpressions;

namespace BMS_API.Helpers
{
    public class EmailValidator
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsValidEmail(string email)
        {
            return EmailRegex.IsMatch(email);
        }
    }
}
