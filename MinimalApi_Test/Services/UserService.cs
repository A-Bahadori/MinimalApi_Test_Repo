using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MinimalApi_Test.DTOs.User;
using MinimalApi_Test.Entities.User;
using MinimalApi_Test.Helpers;
using MinimalApi_Test.Repositories.Interfaces;
using MinimalApi_Test.Result;
using MinimalApi_Test.Services.Interfaces;
using MinimalApi_Test.Validators.User;

namespace MinimalApi_Test.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IMapper _mapper;

        public UserService(
            IGenericRepository<User> userRepository,
            IPasswordHasher<User> passwordHasher,
            IMapper mapper)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ResultCustom<UserDto>> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken)
        {
            var validator = new CreateUserDtoValidator();
            var validationResult = await validator.ValidateAsync(createUserDto, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ResultCustom<UserDto>.Failure($"Validation failed: {errors}");
            }

            try
            {
                await _userRepository.BeginTransactionAsync(cancellationToken);

                var exists = await _userRepository.ExistsAsync(u => u.Username == createUserDto.Username, cancellationToken);
                if (exists)
                {
                    await _userRepository.RollbackTransactionAsync(cancellationToken);
                    return ResultCustom<UserDto>.Failure("Username already exists");
                }

                var user = _mapper.Map<User>(createUserDto);
                user.PasswordHash = _passwordHasher.HashPassword(user, createUserDto.Password);

                var createdUser = await _userRepository.AddAsync(user, cancellationToken);
                var saveResult = await _userRepository.SaveChangesAsync(cancellationToken);

                if (!saveResult)
                {
                    await _userRepository.RollbackTransactionAsync(cancellationToken);
                    return ResultCustom<UserDto>.Failure("Failed to save user");
                }

                await _userRepository.CommitTransactionAsync(cancellationToken);
                return ResultCustom<UserDto>.Success(_mapper.Map<UserDto>(createdUser));
            }
            catch (Exception ex)
            {
                await _userRepository.RollbackTransactionAsync(cancellationToken);
                return ResultCustom<UserDto>.Failure($"Error creating user: {ex.Message}");
            }
        }

        public async Task<ResultCustom<UserDto>> UpdateUserAsync(int id, UpdateUserDto updateUserDto, CancellationToken cancellationToken)
        {
            var validator = new UpdateUserDtoValidator();
            var validationResult = await validator.ValidateAsync(updateUserDto, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ResultCustom<UserDto>.Failure($"Validation failed: {errors}");
            }

            try
            {
                await _userRepository.BeginTransactionAsync(cancellationToken);

                var user = await _userRepository.GetByIdAsync(id, tracking: true, cancellationToken: cancellationToken);
                if (user == null)
                {
                    await _userRepository.RollbackTransactionAsync(cancellationToken);
                    return ResultCustom<UserDto>.Failure("User not found");
                }

                if (user.Id is 1 or 2)
                {
                    await _userRepository.RollbackTransactionAsync(cancellationToken);
                    return ResultCustom<UserDto>.Failure("Unable to access the requested data");
                }

                if (!string.IsNullOrEmpty(updateUserDto.Username) && updateUserDto.Username != user.Username)
                {
                    var exists = await _userRepository.ExistsAsync(u => u.Username == updateUserDto.Username, cancellationToken);
                    if (exists)
                    {
                        await _userRepository.RollbackTransactionAsync(cancellationToken);
                        return ResultCustom<UserDto>.Failure("Username already exists");
                    }
                }

                _mapper.Map(updateUserDto, user);

                if (!string.IsNullOrEmpty(updateUserDto.Password))
                {
                    user.PasswordHash = _passwordHasher.HashPassword(user, updateUserDto.Password);
                }

                var updateResult = await _userRepository.UpdateAsync(user, cancellationToken: cancellationToken);
                var saveResult = await _userRepository.SaveChangesAsync(cancellationToken);

                if (!updateResult || !saveResult)
                {
                    await _userRepository.RollbackTransactionAsync(cancellationToken);
                    return ResultCustom<UserDto>.Failure("Failed to update user");
                }

                await _userRepository.CommitTransactionAsync(cancellationToken);
                return ResultCustom<UserDto>.Success(_mapper.Map<UserDto>(user));
            }
            catch (Exception ex)
            {
                await _userRepository.RollbackTransactionAsync(cancellationToken);
                return ResultCustom<UserDto>.Failure($"Error updating user: {ex.Message}");
            }
        }

        public async Task<ResultCustom<UserDto>> PatchUserAsync(int id, JsonPatchDocument<PatchUserDto> patchDoc, CancellationToken cancellationToken)
        {
            var validator = new PatchUserDtoValidator();
            var validationResult = await validator.ValidateAsync(patchDoc, cancellationToken);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return ResultCustom<UserDto>.Failure($"Validation failed: {errors}");
            }

            try
            {
                await _userRepository.BeginTransactionAsync(cancellationToken);

                var user = await _userRepository.GetByIdAsync(id, tracking: true, cancellationToken: cancellationToken);
                if (user == null)
                {
                    await _userRepository.RollbackTransactionAsync(cancellationToken);
                    return ResultCustom<UserDto>.Failure("User not found");
                }

                if (user.Id is 1 or 2)
                {
                    await _userRepository.RollbackTransactionAsync(cancellationToken);
                    return ResultCustom<UserDto>.Failure("Unable to access the requested data");
                }

                // Create ModelState for validation
                var modelState = new ModelStateDictionary();

                // Method to handle patch errors
                void HandlePatchErrors(JsonPatchError patchError)
                {
                    modelState.TryAddModelError(patchError.AffectedObject?.ToString() ?? string.Empty, patchError.ErrorMessage);
                }

                var userToPatch = _mapper.Map<PatchUserDto>(user);

                // Apply patch operations with error handling
                patchDoc.ApplyTo(userToPatch, HandlePatchErrors);

                // Validate ModelState
                if (!modelState.IsValid)
                {
                    await _userRepository.RollbackTransactionAsync(cancellationToken);
                    var errors = string.Join(", ", modelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return ResultCustom<UserDto>.Failure($"Validation failed: {errors}");
                }

                // Check if username is being changed and if it's unique
                if (patchDoc.Operations.Any(op => op.path.Contains("/Username", StringComparison.OrdinalIgnoreCase)))
                {
                    var exists = await _userRepository.ExistsAsync(u => u.Username == user.Username && u.Id != id, cancellationToken);
                    if (exists)
                    {
                        await _userRepository.RollbackTransactionAsync(cancellationToken);
                        return ResultCustom<UserDto>.Failure("Username already exists");
                    }
                }

                _mapper.Map(userToPatch, user);

                var updateResult = await _userRepository.UpdateAsync(user, cancellationToken: cancellationToken);
                var saveResult = await _userRepository.SaveChangesAsync(cancellationToken);

                if (!updateResult || !saveResult)
                {
                    await _userRepository.RollbackTransactionAsync(cancellationToken);
                    return ResultCustom<UserDto>.Failure("Failed to patch user");
                }

                await _userRepository.CommitTransactionAsync(cancellationToken);
                return ResultCustom<UserDto>.Success(_mapper.Map<UserDto>(user));
            }
            catch (Exception ex)
            {
                await _userRepository.RollbackTransactionAsync(cancellationToken);
                return ResultCustom<UserDto>.Failure($"Error patching user: {ex.Message}");
            }
        }


        public async Task<ResultCustom<bool>> DeleteUserAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _userRepository.BeginTransactionAsync(cancellationToken);

                if (id is 1 or 2)
                {
                    await _userRepository.RollbackTransactionAsync(cancellationToken);
                    return ResultCustom<bool>.Failure("Unable to access the requested data");
                }

                var result = await _userRepository.RemoveByIdAsync(id, hardDelete: false, cancellationToken);
                if (!result)
                {
                    await _userRepository.RollbackTransactionAsync(cancellationToken);
                    return ResultCustom<bool>.Failure("User not found");
                }

                var saveResult = await _userRepository.SaveChangesAsync(cancellationToken);
                if (!saveResult)
                {
                    await _userRepository.RollbackTransactionAsync(cancellationToken);
                    return ResultCustom<bool>.Failure("Failed to delete user");
                }

                await _userRepository.CommitTransactionAsync(cancellationToken);
                return ResultCustom<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await _userRepository.RollbackTransactionAsync(cancellationToken);
                return ResultCustom<bool>.Failure($"Error deleting user: {ex.Message}");
            }
        }

        public async Task<ResultCustom<UserDto>> GetUserByIdAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id, tracking: false, cancellationToken: cancellationToken);
                if (user == null)
                {
                    return ResultCustom<UserDto>.Failure("User not found");
                }

                return ResultCustom<UserDto>.Success(_mapper.Map<UserDto>(user));
            }
            catch (Exception ex)
            {
                return ResultCustom<UserDto>.Failure($"Error retrieving user: {ex.Message}");
            }
        }

        public async Task<ResultCustom<List<UserDto>>> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            try
            {
                var users = await _userRepository.GetAsync(
                    specification: null,
                    tracking: false,
                    cancellationToken: cancellationToken
                );

                return ResultCustom<List<UserDto>>.Success(_mapper.Map<List<UserDto>>(users));
            }
            catch (Exception ex)
            {
                return ResultCustom<List<UserDto>>.Failure($"Error retrieving users: {ex.Message}");
            }
        }

        public async Task<ResultCustom<(bool IsValid, UserDto User)>> ValidateCredentialsAsync(string username, string password, CancellationToken cancellationToken)
        {
            try
            {
                var user = (await _userRepository.GetAsync(
                    specification: u => u.Username == username,
                    tracking: false,
                    cancellationToken: cancellationToken
                )).FirstOrDefault();

                if (user == null)
                {
                    return ResultCustom<(bool, UserDto User)>.Failure("Invalid credentials");
                }

                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

                var userDto = _mapper.Map<UserDto>(user);
                
                return ResultCustom<(bool, UserDto User)>.Success((result == PasswordVerificationResult.Success, userDto));
            }
            catch (Exception ex)
            {
                return ResultCustom<(bool, UserDto User)>.Failure($"Error validating credentials: {ex.Message}");
            }
        }

        public async Task<ResultCustom<SearchUsersResult>> SearchUsersAsync(
            SearchUserDto searchDto,
            CancellationToken cancellationToken)
        {
            try
            {
                // Base specification
                Expression<Func<User, bool>> specification = u => true;

                if (!string.IsNullOrWhiteSpace(searchDto.FirstName))
                {
                    specification = specification.And<User>(u =>
                        u.FirstName.ToLower().Contains(searchDto.FirstName.ToLower()));
                }

                if (!string.IsNullOrWhiteSpace(searchDto.LastName))
                {
                    specification = specification.And<User>(u =>
                        u.LastName.ToLower().Contains(searchDto.LastName.ToLower()));
                }

                if (!string.IsNullOrWhiteSpace(searchDto.Username))
                {
                    specification = specification.And<User>(u =>
                        u.Username.ToLower() == searchDto.Username.ToLower());
                }

                if (!string.IsNullOrWhiteSpace(searchDto.Role))
                {
                    specification = specification.And<User>(u =>
                        u.Role.ToLower() == searchDto.Role.ToLower());
                }

                if (searchDto.CreatedFromDate.HasValue)
                {
                    specification = specification.And<User>(u =>
                        u.CreatedAt >= searchDto.CreatedFromDate.Value);
                }

                if (searchDto.CreatedToDate.HasValue)
                {
                    specification = specification.And<User>(u =>
                        u.CreatedAt <= searchDto.CreatedToDate.Value);
                }

                // Build the ordering
                Func<IQueryable<User>, IOrderedQueryable<User>>? orderBy = null;
                if (!string.IsNullOrWhiteSpace(searchDto.SortBy))
                {
                    orderBy = searchDto.SortBy.ToLower() switch
                    {
                        "firstname" => query => searchDto.SortDescending
                            ? query.OrderByDescending(u => u.FirstName)
                            : query.OrderBy(u => u.FirstName),
                        "lastname" => query => searchDto.SortDescending
                            ? query.OrderByDescending(u => u.LastName)
                            : query.OrderBy(u => u.LastName),
                        "username" => query => searchDto.SortDescending
                            ? query.OrderByDescending(u => u.Username)
                            : query.OrderBy(u => u.Username),
                        "createdat" => query => searchDto.SortDescending
                            ? query.OrderByDescending(u => u.CreatedAt)
                            : query.OrderBy(u => u.CreatedAt),
                        _ => query => query.OrderBy(u => u.Id)
                    };
                }

                int totalCount;
                IReadOnlyList<User> users;

                if (searchDto.IsDeleted ?? false)
                {
                    // Get deleted items by ignoring query filters
                    totalCount = await _userRepository.CountWithDeletedAsync(specification, cancellationToken);
                    users = await _userRepository.GetWithDeletedAsync(
                        specification: specification,
                        orderBy: orderBy,
                        tracking: false,
                        cancellationToken: cancellationToken);

                    // Additional filter for deleted items only
                    users = users.Where(u => u.IsDelete).ToList();
                }
                else
                {
                    // Get non-deleted items (uses default query filter)
                    totalCount = await _userRepository.CountAsync(specification, cancellationToken);
                    users = await _userRepository.GetAsync(
                        specification: specification,
                        orderBy: orderBy,
                        tracking: false,
                        cancellationToken: cancellationToken);
                }

                var pagedUsers = users
                    .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToList();

                var searchUserResult = new SearchUsersResult(_mapper.Map<List<UserDto>>(pagedUsers),totalCount,searchDto.PageNumber,searchDto.PageSize);
                
                return ResultCustom<SearchUsersResult>.Success(searchUserResult);
            }
            catch (Exception ex)
            {
                return ResultCustom<SearchUsersResult>.Failure(
                    $"Error searching users: {ex.Message}");
            }
        }
    }
}
