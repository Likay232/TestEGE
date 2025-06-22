namespace WebApi.Infrastructure.Models.Storage;

public class User : BaseEntity
{
    public string Email { get; set; }
    public string Password { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string TimeZone { get; set; }
    
    public DateTime? LastLogin { get; set; }
    
    public int RoleId { get; set; }
    public Role Role { get; set; }
    public string About { get; set; }

}