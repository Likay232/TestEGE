using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebApi.Infrastructure.Models.Requests;

public class EditExercise
{
    public int Id { get; set; }
    public int Year { get; set; }
    public string Text { get; set; }
    public string Answer { get; set; }
    public string? ExerciseFilePath { get; set; }
    public string? SolutionFilePath { get; set; }
    public int EgeNumber { get; set; }
    
    public bool AttachmentRequired { get; set; }
    public bool ModerationPassed { get; set; }
    public int TeacherId { get; set; }
    
    [ValidateNever]
    public string TeacherLastName { get; set; }
    
    [ValidateNever]
    public string TeacherFirstName { get; set; }
    
    public IFormFile? ExerciseFile { get; set; }
    public IFormFile? SolutionFile { get; set; }
}