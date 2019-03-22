using System;
using System.Linq;

namespace ClearChat.Core
{
    public sealed class UserNameRules
    {
        private static readonly char[] s_ValidNonAlphaNumeric = { '-', '\'', ' ' };
        private static readonly char[] s_AsciiWhitespace = { ' ', '\t', ' ', '\r', '\n', '\v' };

        public (bool isValid, string errorMessage) IsValid(string userName)
        {
            if (userName.Length < 4) return (false, "too short");
            if (userName.Length > 50) return (false, "far too long");
            if (userName.Length > 25) return (false, "too long");
            if (!userName.All(c => char.IsLetterOrDigit(c) || s_ValidNonAlphaNumeric.Contains(c)))
                return (false, "must only contain alpha-numeric characters");
            if (userName.Any(c => c > 255)) return (false, "no funny characters");

            return (true, null);
        }

        public string TrimAndCondense(string userName)
        {
            return string.Join(' ', userName.Split(s_AsciiWhitespace,
                                                   StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
