using Microsoft.AspNetCore.JsonPatch;
using MinimalApi_Test.DTOs.User;
using MinimalApi_Test.Entities.User;
using MinimalApi_Test.Result;

namespace MinimalApi_Test.Services.Interfaces
{
    public interface IUserService
    {
        Task<ResultCustom<UserDto>> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken);
        Task<ResultCustom<UserDto>> UpdateUserAsync(int id, UpdateUserDto updateUserDto, CancellationToken cancellationToken);
        Task<ResultCustom<UserDto>> PatchUserAsync(int id, JsonPatchDocument<PatchUserDto> patchDoc, CancellationToken cancellationToken);
        Task<ResultCustom<bool>> DeleteUserAsync(int id, CancellationToken cancellationToken);
        Task<ResultCustom<UserDto>> GetUserByIdAsync(int id, CancellationToken cancellationToken);
        Task<ResultCustom<List<UserDto>>> GetAllUsersAsync(CancellationToken cancellationToken);
        Task<ResultCustom<(bool IsValid, UserDto User)>> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken);
        Task<ResultCustom<SearchUsersResult>> SearchUsersAsync(SearchUserDto searchDto, CancellationToken cancellationToken);
    }
}
