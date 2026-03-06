using System.Net.Mail;
using Users.Domain.Exceptions;


namespace Users.Domain.ValueObjects
{
    public sealed class Email
    {
        public string Value { get; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Email is required.");

            try
            {
                var address = new MailAddress(value);
                if (address.Address != value)
                    throw new DomainException("Invalid email format.");
            }
            catch
            {
                throw new DomainException("Invalid email format.");
            }

            Value = value;
        }

        public override string ToString() => Value;

        public override bool Equals(object? obj)
            => obj is Email other &&
               Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);

        public override int GetHashCode()
            => Value.ToLower().GetHashCode();
    }
}
