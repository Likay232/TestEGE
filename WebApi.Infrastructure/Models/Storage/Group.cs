namespace WebApi.Infrastructure.Models.Storage;

public class Group : BaseEntity
{
    public string Name { get; set; }
    
    public int TeacherId { get; set; }
    public User Teacher { get; set; }
}