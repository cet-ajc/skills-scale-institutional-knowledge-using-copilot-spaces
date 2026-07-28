using CleanArchitecture.Domain.Common;

namespace CleanArchitecture.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = "User";
    public bool IsActive { get; private set; } = true;

    private User() { }

    public static User Create(string username, string email, string passwordHash, string role = "User")
    {
        return new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true
        };
    }

    public void Update(string username, string email)
    {
        Username = username;
        Email = email;
        SetUpdatedAt();
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }
}
