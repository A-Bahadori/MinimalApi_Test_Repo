namespace MinimalApi_Test.DTOs.User
{
    public record UpdateUserDto
    {
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? Username { get; init; }
        public string? Password { get; init; }
        public string? Role { get; init; }
    }
}
