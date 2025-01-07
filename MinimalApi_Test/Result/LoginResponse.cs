using MinimalApi_Test.DTOs.User;

namespace MinimalApi_Test.Result;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public UserDto? UserDto { get; set; } = null;
}