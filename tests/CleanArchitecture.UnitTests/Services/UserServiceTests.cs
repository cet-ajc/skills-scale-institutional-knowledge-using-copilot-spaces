using CleanArchitecture.Application.DTOs.Requests;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Exceptions;
using CleanArchitecture.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CleanArchitecture.UnitTests.Services;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepoMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<UserService>>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);

        _userService = new UserService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            User.Create("user1", "user1@example.com", "hash1"),
            User.Create("user2", "user2@example.com", "hash2")
        };
        _userRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithExistingId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("testuser", "test@example.com", "hash");
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await _userService.Invoking(s => s.GetUserByIdAsync(userId))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidData_ReturnsUpdatedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("olduser", "old@example.com", "hash");
        var request = new UpdateUserRequest { Username = "newuser", Email = "new@example.com" };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.GetByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _userService.UpdateUserAsync(userId, request);

        // Assert
        result.Username.Should().Be("newuser");
        result.Email.Should().Be("new@example.com");
    }

    [Fact]
    public async Task DeleteUserAsync_WithExistingId_DeletesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _userService.DeleteUserAsync(userId);

        // Assert
        _userRepoMock.Verify(r => r.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WithNonExistingId_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        await _userService.Invoking(s => s.DeleteUserAsync(userId))
            .Should().ThrowAsync<NotFoundException>();
    }
}
