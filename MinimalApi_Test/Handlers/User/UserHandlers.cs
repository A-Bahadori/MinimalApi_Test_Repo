using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using MinimalApi_Test.DTOs.User;
using MinimalApi_Test.Result;
using MinimalApi_Test.Security;
using MinimalApi_Test.Services;
using MinimalApi_Test.Services.Interfaces;

namespace MinimalApi_Test.Handlers.User
{
    public static class UserHandlers
    {
        [Authorize(Roles = "Admin")]
        public static async Task<IResult> GetAllUsers(
            IUserService userService,
            CancellationToken cancellationToken)
        {
            LoggerService.LogInformation("Request received: Get all users");
            var result = await userService.GetAllUsersAsync(cancellationToken);

            if (!result.IsSuccess)
            {
                LoggerService.LogError($"Failed to get all users: {result.Error}");
                return TypedResults.Problem(result.Error);
            }

            LoggerService.LogInformation($"Successfully retrieved {result.Data?.Count ?? 0} users");
            return TypedResults.Ok(result);
        }

        [Authorize(Roles = "Admin")]
        public static async Task<IResult> GetUserById(
            int id,
            IUserService userService,
            CancellationToken cancellationToken)
        {
            LoggerService.LogInformation($"Request received: Get user by ID {id}");
            var result = await userService.GetUserByIdAsync(id, cancellationToken);

            if (!result.IsSuccess)
            {
                LoggerService.LogWarning($"Failed to get user with ID {id}: {result.Error}");
                return TypedResults.Problem(result.Error);
            }

            LoggerService.LogInformation($"Successfully retrieved user with ID {id}");
            return TypedResults.Ok(result);
        }

        [Authorize(Roles = "Admin")]
        public static async Task<IResult> CreateUser(
            CreateUserDto createUserDto,
            IUserService userService,
            CancellationToken cancellationToken)
        {
            LoggerService.LogInformation($"Request received: Create user with username {createUserDto.Username}");
            var result = await userService.CreateUserAsync(createUserDto, cancellationToken);

            if (!result.IsSuccess)
            {
                LoggerService.LogWarning($"Failed to create user: {result.Error}");
                return TypedResults.Problem(result.Error);
            }

            LoggerService.LogInformation($"Successfully created user with ID {result.Data?.Id}");
            return TypedResults.Created($"/api/users/{result.Data?.Id}", result);
        }

        [Authorize(Roles = "Admin")]
        public static async Task<IResult> UpdateUser(
            int id,
            UpdateUserDto updateUserDto,
            IUserService userService,
            CancellationToken cancellationToken)
        {
            LoggerService.LogInformation($"Request received: Update user with ID {id}");
            var result = await userService.UpdateUserAsync(id, updateUserDto, cancellationToken);

            if (!result.IsSuccess)
            {
                LoggerService.LogWarning($"Failed to update user with ID {id}: {result.Error}");
                return TypedResults.Problem(result.Error);
            }

            LoggerService.LogInformation($"Successfully updated user with ID {id}");
            return TypedResults.Ok(result);
        }

        [Authorize(Roles = "Admin")]
        public static async Task<IResult> DeleteUser(
            int id,
            IUserService userService,
            CancellationToken cancellationToken)
        {
            LoggerService.LogInformation($"Request received: Delete user with ID {id}");
            var result = await userService.DeleteUserAsync(id, cancellationToken);

            if (!result.IsSuccess)
            {
                LoggerService.LogWarning($"Failed to delete user with ID {id}: {result.Error}");
                return TypedResults.Problem(result.Error);
            }

            LoggerService.LogInformation($"Successfully deleted user with ID {id}");
            return TypedResults.Ok(result);
        }

        public static async Task<IResult> Login(
            Models.LoginRequest request,
            IUserService userService,
            CancellationToken cancellationToken,
            IConfiguration configuration)
        {
            LoggerService.LogInformation($"Login attempt for user: {request.Username}");

            var result = await userService.ValidateCredentialsAsync(
                request.Username,
                request.Password,
                cancellationToken);

            if (!result.IsSuccess || !result.Data.IsValid)
            {
                LoggerService.LogWarning($"Failed login attempt for user: {request.Username}");
                return TypedResults.Unauthorized();
            }

            var token = JwtSecurity.GenerateJwtToken(request.Username, result.Data.Role ?? "User", configuration);
            LoggerService.LogInformation($"Successful login for user: {request.Username}");
            return TypedResults.Ok(new { Token = token });
        }

        [Authorize(Roles = "Admin")]
        public static async Task<IResult> SearchUsers(
            SearchUserDto searchDto,
            IUserService userService,
            CancellationToken cancellationToken)
        {
            LoggerService.LogInformation("Request received: Search users");
            var result = await userService.SearchUsersAsync(searchDto, cancellationToken);

            if (!result.IsSuccess)
            {
                LoggerService.LogWarning($"Failed to search users: {result.Error}");
                return TypedResults.Problem(result.Error);
            }

            LoggerService.LogInformation($"Successfully searched users. Found {result.Data.Items.Count} items");
            return TypedResults.Ok(new SearchUsersResult(result.Data));
        }

        [Authorize(Roles = "Admin")]
        public static async Task<IResult> PatchUser(
            int id,
            [FromBody] JsonPatchDocument<Entities.User.User> patchDoc,
            IUserService userService,
            CancellationToken cancellationToken)
        {
            LoggerService.LogInformation($"Request received: Patch user with ID {id}");
            var result = await userService.PatchUserAsync(id, new JsonPatchDocument<PatchUserDto>(), cancellationToken);

            if (!result.IsSuccess)
            {
                LoggerService.LogWarning($"Failed to patch user with ID {id}: {result.Error}");
                return TypedResults.Problem(result.Error);
            }

            LoggerService.LogInformation($"Successfully patched user with ID {id}");
            return TypedResults.Ok(result);
        }
    }
}