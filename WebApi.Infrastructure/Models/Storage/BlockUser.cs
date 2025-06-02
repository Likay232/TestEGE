namespace WebApi.Infrastructure.Models.Storage;

public class BlockUser : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; }
    
    public DateTime BlockedUntil { get; set; }
}