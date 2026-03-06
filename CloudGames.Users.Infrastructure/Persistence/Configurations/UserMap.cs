using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Infrastructure.Persistence.Configurations
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);

            var emailConverter = new ValueConverter<Email, string>(
                email => email.Value,
                value => new Email(value)
            );

            builder.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Value)
                     .HasColumnName("Email")
                     .IsRequired()
                     .HasMaxLength(150);
            });

            var nameConverter = new ValueConverter<Name, string>(
                name => name.Value,
                value => new Name(value)
            );

            builder.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100);

            builder.Property(x => x.PasswordHash)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(x => x.Role)
                   .IsRequired();

            builder.Property(x => x.IsActive)
                   .IsRequired();
        }
    }
}