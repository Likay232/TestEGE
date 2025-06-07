using Microsoft.AspNetCore.Http;

namespace WebApi.Infrastructure.Models.Requests;

public class CheckExercise
{
    public int UserId { get; set; }
    public int ExerciseId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public IFormFile? Attachment { get; set; }
}