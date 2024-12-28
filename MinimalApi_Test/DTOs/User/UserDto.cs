namespace MinimalApi_Test.DTOs.User
{
    public record UserDto
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime? ModifiedAt { get; init; }
    }
}
