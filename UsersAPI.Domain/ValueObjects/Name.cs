using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Domain.ValueObjects
{
    public class Name
    {
        public string Value { get; }
        public Name(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Nome is required.");

            if (value.Length < 2)
                throw new ArgumentException("Nome must be at least 2 characters.");

          

            Value = value;
        }
    }
}
