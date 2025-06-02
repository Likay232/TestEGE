namespace WebApi.Infrastructure.Models.Storage;

public class Variant : BaseEntity
{
    public string Title { get; set; }
    
    public int TeacherId { get; set; }
    public User Teacher { get; set; }
}