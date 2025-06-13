using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Infrastructure.Models.Storage;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public class StudentController(StudentService service) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> AllExercises()
    {
        return View(await service.GetAllExercises());
    }

    [HttpGet]
    public async Task<IActionResult> GetExercise(int id)
    {
        return View(await service.GetExercise(id));
    }


    [HttpGet]
    public async Task<IActionResult> AllVariants()
    {
        return View(await service.GetAllVariants());
    }

    [HttpGet]
    public async Task<IActionResult> MyVariants()
    {
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        return View(await service.GetAssignedVariants(userId));
    }

    [HttpGet]
    public async Task<IActionResult> GetVariant(int variantId)
    {
        return View(await service.GetVariant(variantId));
    }

    [HttpGet]
    public async Task<IActionResult> GetVariantToSolve(int variantId)
    {
        ViewBag.StudentId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var variant = await service.GetVariant(variantId);

        return View(variant);
    }

    [HttpPost]
    public async Task<IActionResult> GetVariantToSolve([FromForm] VariantForCheck variant)
    {
        var checkedVariant = await service.CheckVariant(variant);

        return View("CheckedVariant", checkedVariant);
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

    [HttpGet]
    public async Task<IActionResult> GetExamToSolve(int variantId)
    {
        ViewBag.StudentId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var variant = await service.GetVariant(variantId);

        return View(variant);
    }

    [HttpPost]
    public async Task<IActionResult> GetExamToSolve([FromForm] VariantForCheck variant)
    {
        var checkedVariant = await service.CheckVariant(variant);

        return View("CheckedVariant", checkedVariant);
    }
    
    [HttpGet]
    public async Task<IActionResult> DownloadFileFromRepo(string filePath)
    {
        try
        {
            var fileBytes = await service.GetFileBytes(filePath);
            
            if (fileBytes == null) return NotFound();
            
            return File(fileBytes, "application/octet-stream", filePath);
        }
        catch (Exception e)
        {
            return StatusCode(500, e);
        }
    }

    public async Task<IActionResult> DeleteAccount()
    {
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        if (await service.DeleteAccount(userId))
            return RedirectToAction("Login", "Auth");

        return RedirectToAction("Index");
    }
}