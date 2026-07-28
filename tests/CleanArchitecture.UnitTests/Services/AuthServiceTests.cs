using CleanArchitecture.Application.DTOs.Requests;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Exceptions;
using CleanArchitecture.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CleanArchitecture.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepoMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);

        _authService = new AuthService(
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.GetByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _passwordHasherMock.Setup(p => p.Hash(request.Password)).Returns("hashed_password");
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _tokenServiceMock.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns("test_token");
        _tokenServiceMock.Setup(t => t.GetTokenExpiration()).Returns(DateTime.UtcNow.AddHours(1));

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("test_token");
        result.User.Email.Should().Be(request.Email);
        result.User.Username.Should().Be(request.Username);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsValidationException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "existing@example.com",
            Password = "Password123!"
        };
        var existingUser = User.Create("existing", "existing@example.com", "hash");
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await _authService.Invoking(s => s.RegisterAsync(request))
            .Should().ThrowAsync<Domain.Exceptions.ValidationException>()
            .WithMessage("One or more validation errors occurred.");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };
        var user = User.Create("testuser", "test@example.com", "hashed_password");

        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify(request.Password, user.PasswordHash)).Returns(true);
        _tokenServiceMock.Setup(t => t.GenerateToken(user)).Returns("test_token");
        _tokenServiceMock.Setup(t => t.GetTokenExpiration()).Returns(DateTime.UtcNow.AddHours(1));

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("test_token");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };
        var user = User.Create("testuser", "test@example.com", "hashed_password");

        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify(request.Password, user.PasswordHash)).Returns(false);

        // Act & Assert
        await _authService.Invoking(s => s.LoginAsync(request))
            .Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ThrowsUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "notfound@example.com",
            Password = "Password123!"
        };
        _userRepoMock.Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await _authService.Invoking(s => s.LoginAsync(request))
            .Should().ThrowAsync<UnauthorizedException>();
    }
}
