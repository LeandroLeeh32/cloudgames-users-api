using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Exceptions;

namespace Users.Domain.ValueObjects
{
    public sealed class Password
    {
        public string Value { get; }

        public Password(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Password is required.");

            if (value.Length < 8)
                throw new DomainException("Password must be at least 8 characters.");

            if (!value.Any(char.IsLetter) ||
                !value.Any(char.IsDigit) ||
                !value.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                throw new DomainException(
                    "Password must contain at least one letter, one number and one special character.");
            }

            Value = value;
        }
    }
}
