using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Components;
using WebApi.Infrastructure.Models.DTO;

namespace WebApi.Services;

public class TeacherService(DataComponent component)
{
    public async Task<ProfileInfo> GetProfileInfo(int userId)
    {
        return await component.Users
            .Select(u => new ProfileInfo
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Password = u.Password,
                DateOfBirth = u.DateOfBirth,
                TimeZone = u.TimeZone,
                About = u.About
            })
            .FirstAsync(x => x.Id == userId);
    }

    public async Task<bool> EditProfileInfo(ProfileInfo profileInfo)
    {
        var userEntry = await component.Users
            .FirstOrDefaultAsync(u => u.Id == profileInfo.Id);
        if (userEntry == null) return false;
        
        userEntry.FirstName = profileInfo.FirstName;
        userEntry.LastName = profileInfo.LastName;
        userEntry.Email = profileInfo.Email;
        userEntry.Password = profileInfo.Password;
        userEntry.DateOfBirth = DateTime.SpecifyKind(profileInfo.DateOfBirth, DateTimeKind.Utc);
        userEntry.TimeZone = profileInfo.TimeZone;
        userEntry.About = profileInfo.About;
        
        return await component.Update(userEntry);
    }
}