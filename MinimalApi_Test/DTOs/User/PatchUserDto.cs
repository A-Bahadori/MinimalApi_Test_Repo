using Microsoft.AspNetCore.JsonPatch;

namespace MinimalApi_Test.DTOs.User
{
    public class PatchUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Role { get; set; }
    }

}
