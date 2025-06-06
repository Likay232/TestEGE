namespace WebApi.Infrastructure.Models.Requests;

public class EditGroup
{
    public int GroupId { get; set; }
    public string GroupName { get; set; }
    public List<int>? StudentIds { get; set; }
}