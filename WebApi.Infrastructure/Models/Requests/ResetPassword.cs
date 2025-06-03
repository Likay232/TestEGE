namespace WebApi.Infrastructure.Models.Requests;

public class ResetPassword
{
    public string NewPassword { get; set; }
    public string Token { get; set; }
}