namespace WebApi.Infrastructure.Models.Storage;

public class Group : BaseEntity
{
    public int TeacherId { get; set; }
    public User Teacher { get; set; }
}