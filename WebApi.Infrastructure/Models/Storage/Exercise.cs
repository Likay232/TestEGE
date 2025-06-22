namespace WebApi.Infrastructure.Models.Storage;

public class Exercise : BaseEntity
{
    public int Year { get; set; }
    public string Text { get; set; }
    public string Answer { get; set; }
    public string ExerciseFilePath { get; set; }
    public string SolutionFilePath { get; set; }
    public int EgeNumber { get; set; }
    
    public int PrimaryScore { get; set; }
    
    public bool AttachmentRequired { get; set; }
    public bool ModerationPassed { get; set; }
    
    public int TeacherId { get; set; }
    public User Teacher { get; set; }
}