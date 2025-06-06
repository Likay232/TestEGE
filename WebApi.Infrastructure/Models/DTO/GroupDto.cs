namespace WebApi.Infrastructure.Models.DTO;

public class GroupDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TeacherId { get; set; }
    public string TeacherFirstName { get; set; }
    public string TeacherLastName { get; set; }
    
    public List<UserDto> Students { get; set; }
}