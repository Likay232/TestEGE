using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebApi.Infrastructure.Models.DTO;

public class VariantDto
{
    public int Id { get; set; } 
    public string Title { get; set; }
    public int TeacherId { get; set; }
    public string TeacherLastName { get; set; }
    public string TeacherFirstName { get; set; }
    public List<ExerciseDto> Exercises { get; set; }
    public List<UserDto> AssignedUsers { get; set; }
}