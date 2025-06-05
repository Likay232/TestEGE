namespace WebApi.Infrastructure.Models.Requests;

public class EditUser
{
    public int Id { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }
    
    public string LastName { get; set; }

    public string FirstName { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string TimeZone { get; set; }

    public string RoleName { get; set; }

    public string About { get; set; }

    public DateTime? BlockedUntil { get; set; }
}