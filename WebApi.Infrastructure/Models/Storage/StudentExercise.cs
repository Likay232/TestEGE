namespace WebApi.Infrastructure.Models.Storage;

public class StudentExercise : BaseEntity
{
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; }
    
    public int StudentId { get; set; }
    public User Student { get; set; }
    
    public string StudentAnswer { get; set; }
    public string StudentSolutionFilePath { get; set; }
}