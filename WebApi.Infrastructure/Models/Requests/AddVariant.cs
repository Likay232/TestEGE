using WebApi.Infrastructure.Models.DTO;

namespace WebApi.Infrastructure.Models.Requests;

public class AddVariant
{
    public string Title { get; set; }
    public int TeacherId { get; set; }
    public List<int>? Exercises { get; set; }
    public List<int>? AssignedUsers { get; set; }
}