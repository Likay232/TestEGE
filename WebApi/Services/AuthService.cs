using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApi.Infrastructure.Components;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Infrastructure.Models.Storage;

namespace WebApi.Services;

public class AuthService(DataComponent component)
{
    public async Task<string?> LoginAdmin(Login request)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "super_secret_key_123456789011";

        Console.WriteLine(key);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Name, request.Email),
                new Claim(ClaimTypes.Role, "Admin")
            ]),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public async Task<string?> Login(Login request)
    {
        if (request is { Email: "admin", Password: "admin123" })
            return await LoginAdmin(request);

        var user = await component.Users.FirstOrDefaultAsync(u =>
            u.Email == request.Email && u.Password == request.Password);

        if (user == null)
            return null;

        var blockEntry = await component.BlockUsers
            .FirstOrDefaultAsync(b => b.UserId == user.Id);

        if (blockEntry != null && blockEntry.BlockedUntil < DateTime.UtcNow)
            return null;

        if (blockEntry != null)
            await component.Delete<BlockUser>(blockEntry.Id);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "super_secret_key_123456789011";

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, request.Email),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            ]),
            Expires = DateTime.UtcNow.AddHours(3),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}