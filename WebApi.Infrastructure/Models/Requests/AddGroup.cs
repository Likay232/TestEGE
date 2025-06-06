namespace WebApi.Infrastructure.Models.Requests;

public class AddGroup
{
    public string Name { get; set; }
    
    public int TeacherId { get; set; }
    
    public List<int>? StudentIds { get; set; }
}