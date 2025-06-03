using Microsoft.AspNetCore.Mvc;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthController(AuthService service) : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromForm] Login request)
    {
        try
        {
            var loginResult = await service.Login(request);

            if (loginResult == null)
                return RedirectToAction("Login", "Auth");

            if (string.IsNullOrEmpty(loginResult.Token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            };

            Response.Cookies.Append("AuthToken", loginResult.Token, cookieOptions);

            return RedirectToAction("Index", loginResult.RoleName);
        }
        catch
        {
            return RedirectToAction("Login", "Auth");
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromForm] Register request)
    {
        try
        {
            var registerResult = await service.Register(request);

            if (!registerResult)
                return RedirectToAction("Register", "Auth");

            return RedirectToAction("Login", "Auth");
        }
        catch (Exception e)
        {
            return RedirectToAction("Register", "Auth");
        }
    }

    [HttpGet]
    public IActionResult SendResetMessage()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendResetMessage([FromForm] string email)
    {
        try
        {
            if (await service.SendResetMessage(email))
                ViewBag.Message = "Сообщение для сброса пароля отправлено на почту.";
            else
                ViewBag.Message = "Ошибка при отправке сообщения.";
        }
        catch (Exception e)
        {
            ViewBag.Message = e.Message;
        }

        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword([FromQuery] string token, [FromForm] string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Message = "Пароль не может быть пустым.";
            return View();
        }
        
        var request = new ResetPassword
        {
            Token = token,
            NewPassword = password
        };

        if (await service.ResetPassword(request))
            ViewBag.Message = "Пароль успешно сброшен.";
        else
            ViewBag.Message = "Ошибка при сбросе пароля.";

        return View();
    }
}