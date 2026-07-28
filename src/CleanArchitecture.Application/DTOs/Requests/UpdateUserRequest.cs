using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Application.DTOs.Requests;

public class UpdateUserRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
