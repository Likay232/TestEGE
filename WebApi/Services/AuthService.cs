using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApi.Infrastructure.Components;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Infrastructure.Models.Storage;

namespace WebApi.Services;

public class AuthService(DataComponent component)
{
    private async Task<LoginResult?> LoginAdmin(Login request)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "super_secret_key_123456789011";

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

        return new LoginResult
        {
            Token = tokenHandler.WriteToken(token),
            RoleName = "Admin",
        };
    }

    public async Task<bool> Register(Register request)
    {
        if (request.Password != request.ConfirmPassword)
            throw new Exception("Пароли не совпадают.");

        var roleEntry = await component.Roles
            .FirstOrDefaultAsync(r => r.RoleName == request.Role);

        if (roleEntry == null)
            throw new Exception("Роль с таким названием не найдена.");

        var user = await component.Users.FirstOrDefaultAsync(u =>
            u.Email == request.Email);

        if (user != null)
            throw new Exception("Имя пользователя занято.");

        var newUser = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password,
            RoleId = roleEntry.Id,
            DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth, DateTimeKind.Utc),
            TimeZone = request.TimeZone,
            About = request.AboutMe
        };

        return await component.Insert(newUser);
    }


    public async Task<LoginResult?> Login(Login request)
    {
        if (request.Email == "admin" && request.Password == "admin123")
            return await LoginAdmin(request);

        var user = await component.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password);

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
        return new LoginResult
        {
            Token = tokenHandler.WriteToken(token),
            RoleName = user.Role.RoleName,
        };
    }

    public async Task<bool> SendResetMessage(string email)
    {
        if (!await component.Users.AnyAsync(u => u.Email == email))
            throw new Exception("Пользователь с данным адресом электронной почты не найден.");

        var token = Guid.NewGuid().ToString();

        var newEntry = new ResetPass
        {
            Token = token,
            UserId = await component.Users
                .Where(u => u.Email == email)
                .Select(u => u.Id)
                .FirstAsync(),
            Expires = DateTime.UtcNow.AddMinutes(10),
        };

        if (!await component.Insert(newEntry))
            return false;

        var message = new MailMessage("noreply@example.com", email);

        message.Subject = "Password Reset";
        message.Body = $"Ссылка для восстановления пароля: http://localhost:5000/Auth/ResetPassword?token={token}";
        try
        {
            using var client = new SmtpClient();
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }

        return true;
    }

    public async Task<bool> ResetPassword(ResetPassword request)
    {
        var record = await component.ResetPasses
            .FirstOrDefaultAsync(r => r.Token == request.Token);

        if (record == null || record.Expires < DateTime.UtcNow)
            return false;

        var userId = record.UserId;

        var user = await component.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        user.Password = request.NewPassword;

        if (await component.Update(user))
        {
            await component.Delete<ResetPass>(record.Id);
            return true;
        }
        return false;
    }
}