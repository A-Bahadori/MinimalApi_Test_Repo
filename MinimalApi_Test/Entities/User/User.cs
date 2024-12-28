using MinimalApi_Test.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace MinimalApi_Test.Entities.User
{
    public class User : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Role { get; set; } = string.Empty;
    }
}
