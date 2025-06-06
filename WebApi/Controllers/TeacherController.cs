using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
[Authorize]
public class TeacherController(TeacherService service) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ProfileInfo()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        Console.WriteLine(userId);
        
        return View(await service.GetProfileInfo(Convert.ToInt32(userId)));
    }

    [HttpPost]
    public async Task<IActionResult> ProfileInfo([FromForm] ProfileInfo profileInfo)
    {
        if (await service.EditProfileInfo(profileInfo))
            return RedirectToAction(nameof(Index));

        ViewBag.Message = "Ошибка при редактировании профиля.";
        
        return View(profileInfo);
    }
}