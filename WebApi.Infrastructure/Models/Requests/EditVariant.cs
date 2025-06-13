namespace WebApi.Infrastructure.Models.Requests;

public class EditVariant
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public string Title { get; set; } = String.Empty;
    public List<int> ExerciseIds { get; set; } = new();
    public List<int> StudentIds { get; set; } = new();
}