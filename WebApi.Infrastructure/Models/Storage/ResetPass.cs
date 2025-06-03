namespace WebApi.Infrastructure.Models.Storage;

public class ResetPass : BaseEntity
{
    public int UserId { get; set; }
    public string Token { get; set; }
    public DateTime Expires { get; set; }
}