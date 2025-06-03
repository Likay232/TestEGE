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

}