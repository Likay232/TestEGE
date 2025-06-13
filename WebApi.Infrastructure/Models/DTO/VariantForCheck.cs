using Microsoft.AspNetCore.Http;

namespace WebApi.Infrastructure.Models.DTO;

public class VariantForCheck
{
    public int StudentId { get; set; }
    public int VariantId { get; set; }
    
    public List<UserAnswer> Answers { get; set; }
}

public class UserAnswer
{
    public int ExerciseId { get; set; }
    public string? Answer { get; set; }
    public IFormFile? Attachment { get; set; }
}