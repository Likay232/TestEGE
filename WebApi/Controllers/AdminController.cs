using Microsoft.AspNetCore.Mvc;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Infrastructure.Models.Storage;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AdminController(AdminService service) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        return View(await service.GetUsers());
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(int userId)
    {
        return View(await service.GetUserToEdit(userId));
    }

    [HttpPost]
    public async Task<IActionResult> EditUser([FromForm] EditUser user)
    {
        if (await service.EditUser(user))
            return RedirectToAction("Users");

        ViewBag.Message = "Ошибка обновления пользователя.";
        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Exercises()
    {
        return View(await service.GetExercises());
    }

    [HttpGet]
    public async Task<IActionResult> GetExercise(int id)
    {
        return View(await service.GetExerciseToEdit(id));
    }

    [HttpGet]
    public async Task<IActionResult> EditExercise(int id)
    {
        ViewBag.Teachers = await service.GetTeachers();

        return View(await service.GetExerciseToEdit(id));
    }

    [HttpPost]
    public async Task<IActionResult> EditExercise(EditExercise updatedExercise)
    {
        if (await service.EditExercise(updatedExercise))
            return RedirectToAction("Exercises");

        ViewBag.Teachers = await service.GetTeachers();
        
        return View(updatedExercise);
    }

    [HttpGet]
    public async Task<IActionResult> ExercisesToModerate()
    {
        return View(await service.GetExercisesToModerate());
    }

    [HttpGet]
    public async Task<IActionResult> ModerateExercise(bool approved, int exerciseId)
    {
        try
        {
            await service.ModerateExercise(new Moderation
            {
                Approved = approved,
                ExerciseId = exerciseId
            });
        }
        catch (Exception)
        {
            return RedirectToAction("Exercises");
        }

        return RedirectToAction("Exercises");
    }

    [HttpGet]
    public async Task<IActionResult> Variants()
    {
        return View(await service.GetVariants());
    }

    [HttpGet]
    public async Task<IActionResult> GetVariant(int variantId)
    {
        try
        {
            var variant = await service.GetVariant(variantId);
            return View(variant);
        }
        catch (Exception)
        {
            return RedirectToAction("Variants");
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditVariant(int variantId)
    {
        ViewBag.Teachers = await service.GetTeachers();
        ViewBag.AllExercises = await service.GetExercises();
        ViewBag.AllStudents = await service.GetStudents();

        return View(await service.GetVariant(variantId));
    }

    [HttpPost]
    public async Task<IActionResult> EditVariant([FromForm] VariantDto updatedVariant)
    {
        if (await service.EditVariant(updatedVariant))
            return RedirectToAction("Variants");
        
        ViewBag.Teachers = await service.GetTeachers();
        ViewBag.AllExercises = await service.GetExercises();
        ViewBag.AllStudents = await service.GetStudents();
        
        return View(updatedVariant);
    }

    [HttpGet]
    public async Task<IActionResult> DeleteVariant(int variantId)
    {
        if (await service.DeleteVariant(variantId))
            return RedirectToAction("Index");

        ViewBag.Message = "Ошибка при удалении варианта.";

        return RedirectToAction("Variants");
    }

    [HttpGet]
    public async Task<IActionResult> DeleteExercise(int exerciseId)
    {
        if (await service.DeleteExercise(exerciseId))
            return RedirectToAction("Index");

        ViewBag.Message = "Ошибка при удалении задания.";

        return RedirectToAction("Exercises");
    }

    [HttpGet]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        if (await service.DeleteUser(userId))
            return RedirectToAction("Index");

        ViewBag.Message = "Ошибка при удалении задания.";

        return RedirectToAction("Users");
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

    [HttpGet]
    public IActionResult ExitFromAccount()
    {
        Response.Cookies.Delete("AuthToken");

        return RedirectToAction("Login", "Auth");
    }


}