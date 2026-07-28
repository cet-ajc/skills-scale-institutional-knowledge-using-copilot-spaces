using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.UnitTests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ThenGetByEmail_ReturnsUser()
    {
        // Arrange
        var user = User.Create("testuser", "test@example.com", "hash");

        // Act
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        var found = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        found.Should().NotBeNull();
        found!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        await _repository.AddAsync(User.Create("user1", "user1@example.com", "hash1"));
        await _repository.AddAsync(User.Create("user2", "user2@example.com", "hash2"));
        await _context.SaveChangesAsync();

        // Act
        var users = await _repository.GetAllAsync();

        // Assert
        users.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        var user = User.Create("testuser", "test@example.com", "hash");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(user.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
    {
        // Act
        var exists = await _repository.ExistsAsync(Guid.NewGuid());

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesUser()
    {
        // Arrange
        var user = User.Create("testuser", "test@example.com", "hash");
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(user.Id);
        await _context.SaveChangesAsync();

        var exists = await _repository.ExistsAsync(user.Id);

        // Assert
        exists.Should().BeFalse();
    }

    public void Dispose() => _context.Dispose();
}
