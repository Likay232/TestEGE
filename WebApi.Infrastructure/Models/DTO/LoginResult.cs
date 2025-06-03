namespace WebApi.Infrastructure.Models.DTO;

public class LoginResult
{
    public string? Token { get; set; }
    public string RoleName { get; set; } = string.Empty;
}