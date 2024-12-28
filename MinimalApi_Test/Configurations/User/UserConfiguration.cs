using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MinimalApi_Test.Configurations.User
{
    public class UserConfiguration:IEntityTypeConfiguration<Entities.User.User>
    {
        public void Configure(EntityTypeBuilder<Entities.User.User> builder)
        {
            var passwordHasher = new PasswordHasher<Entities.User.User>();

            #region Seed Data

            var adminUser = new Entities.User.User()
            {
                Id = 1,
                FirstName = "مدیر",
                LastName = "سیستم",
                Username = "admin@localhost.com",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "P@sswordABAdmin%");

            var normalUser = new Entities.User.User()
            {
                Id = 2,
                FirstName = "کاربر",
                LastName = "عادی",
                Username = "user@localhost.com",
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            normalUser.PasswordHash = passwordHasher.HashPassword(normalUser, "P@sswordABUser%"); 

            #endregion

            builder.HasData(adminUser, normalUser);
        }
    }
}
