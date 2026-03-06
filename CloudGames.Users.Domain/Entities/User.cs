
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Enums;
using Users.Domain.Exceptions;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Email Email { get; private set; }
        public string PasswordHash { get; private set; }
        public UserRole Role { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsActive { get; private set; }

        private User() { } // EF

        private User(string name, Email email, string passwordHash, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Name is required.");

            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        public static User Create(string name,string email,string passWord, UserRole role)
        {
   
            var emailVo = new Email(email);
            var nameVo = new Name(name);
            var passwordHashVo = new Password(passWord);

            return new User(nameVo.Value, emailVo, passwordHashVo.Value, role);
        }

        public void Update(string name, string email, UserRole role)
        {
            Name = new Name(email).Value;
            Email = new Email(email);
            Role = role;
        }

        public void Deactivate()
        {
            if (!IsActive)
                throw new DomainException("User already inactive.");

            IsActive = false;
        }
    }

}
