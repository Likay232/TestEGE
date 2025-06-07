using Microsoft.AspNetCore.Http;

namespace WebApi.Infrastructure.Models.Requests;

public class AddExercise
{
    public int Year { get; set; }
    public string Text { get; set; }
    public string Answer { get; set; }
    public string? ExerciseFilePath { get; set; }
    public string? SolutionFilePath { get; set; }
    public int EgeNumber { get; set; }
    
    public bool AttachmentRequired { get; set; }
    public int TeacherId { get; set; }
    
    public IFormFile? ExerciseFile { get; set; }
    public IFormFile? SolutionFile { get; set; }


}