namespace WebApi.Infrastructure.Models.Requests;

public class Register
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? AboutMe { get; set; } = string.Empty;
}