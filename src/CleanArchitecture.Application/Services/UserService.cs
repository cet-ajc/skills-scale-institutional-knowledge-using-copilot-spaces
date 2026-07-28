using CleanArchitecture.Application.DTOs.Requests;
using CleanArchitecture.Application.DTOs.Responses;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Exceptions;
using CleanArchitecture.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all users");
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        return users.Select(MapToResponse);
    }

    public async Task<UserResponse> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching user with ID: {UserId}", id);
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), id);

        return MapToResponse(user);
    }

    public async Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating user with ID: {UserId}", id);
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), id);

        var existingEmail = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (existingEmail is not null && existingEmail.Id != id)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Email", ["A user with this email already exists."] }
            });

        var existingUsername = await _unitOfWork.Users.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUsername is not null && existingUsername.Id != id)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Username", ["This username is already taken."] }
            });

        user.Update(request.Username, request.Email);
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} updated successfully", id);
        return MapToResponse(user);
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting user with ID: {UserId}", id);
        var exists = await _unitOfWork.Users.ExistsAsync(id, cancellationToken);
        if (!exists)
            throw new NotFoundException(nameof(Domain.Entities.User), id);

        await _unitOfWork.Users.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User {UserId} deleted successfully", id);
    }

    private static UserResponse MapToResponse(Domain.Entities.User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}
