using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Requests;
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
    
    [HttpGet]
    public async Task<IActionResult> MyExercises()
    {
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        return View(await service.GetExercises(userId));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetExercise(int id)
    {
        return View(await service.GetExerciseToEdit(id));
    }

    [HttpGet]
    public async Task<IActionResult> EditExercise(int id)
    {
        return View(await service.GetExerciseToEdit(id));
    }
    
    [HttpPost]
    public async Task<IActionResult> EditExercise([FromForm] EditExercise updatedExercise)
    {
        if (await service.EditExercise(updatedExercise))
            return View(await service.GetExerciseToEdit(updatedExercise.Id));
        
        return RedirectToAction("MyExercises");
    }

    [HttpGet]
    public IActionResult AddExercise()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddExercise([FromForm] AddExercise newExercise)
    {
        var teacherId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        newExercise.TeacherId = teacherId;
        
        if (await service.AddExercise(newExercise))
            return RedirectToAction(nameof(MyExercises));
        
        ViewBag.Message = "Ошибка при добавлении задания.";
        
        return View(newExercise);
    }

    [HttpGet]
    public async Task<IActionResult> MyVariants()
    {
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        return View(await service.GetVariants(userId));
    }

    [HttpGet]
    public async Task<IActionResult> GetVariant(int variantId)
    {
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        return View(await service.GetVariant(variantId, userId));
    }

    [HttpGet]
    public async Task<IActionResult> EditVariant(int variantId)
    {
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        ViewBag.AllExercises = await service.GetExercises(userId);
        ViewBag.AllStudents = await service.GetStudents();

        return View(await service.GetVariant(variantId, userId));
    }

    [HttpGet]
    public async Task<IActionResult> AddVariant()
    {
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        ViewBag.AllExercises = await service.GetExercises(userId);
        ViewBag.AllStudents = await service.GetStudents();
        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddVariant([FromForm] AddVariant newVariant)
    {
        if (newVariant.Exercises.Count == 0)
        {
            ViewBag.Message = "Не назначены задания для варианта.";
        }

        if (newVariant.AssignedUsers.Count == 0)
        {
            ViewBag.Message = "Не назначены студенты для варианта.";
        }
        
        var teacherId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        newVariant.TeacherId = teacherId;
        
        if (await service.AddVariant(newVariant))
            return RedirectToAction(nameof(MyExercises));
        
        ViewBag.Message = "Ошибка при добавлении варианта.";
        
        return View(newVariant);
    }
}