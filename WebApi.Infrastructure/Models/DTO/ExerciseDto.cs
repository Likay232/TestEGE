namespace WebApi.Infrastructure.Models.DTO;

public class ExerciseDto
{
    public int Id { get; set; }
    public int Year { get; set; }
    public string Text { get; set; }
    public string Answer { get; set; }
    public string ExerciseFilePath { get; set; }
    public string SolutionFilePath { get; set; }
    public int EgeNumber { get; set; }
    
    public bool AttachmentRequired { get; set; }
    public bool ModerationPassed { get; set; }
    public int TeacherId { get; set; }
    public string TeacherLastName { get; set; }
    public string TeacherFirstName { get; set; }
}