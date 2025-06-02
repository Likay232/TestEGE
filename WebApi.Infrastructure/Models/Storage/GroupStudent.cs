namespace WebApi.Infrastructure.Models.Storage;

public class GroupStudent : BaseEntity
{
    public int GroupId { get; set; }
    public Group Group { get; set; }
    
    public int StudentId { get; set; }
    public User Student { get; set; }
}