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
        
        return View(await service.GetMyExercises(userId));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetExercise(int id)
    {
        ViewBag.TeacherId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
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
            return RedirectToAction("MyExercises");
        
        return View(updatedExercise);
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
        
        return View(await service.GetMyVariants(userId));
    }

    [HttpGet]
    public async Task<IActionResult> GetVariant(int variantId)
    {
        ViewBag.TeacherId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        return View(await service.GetVariant(variantId));
    }

    [HttpGet]
    public async Task<IActionResult> EditVariant(int variantId)
    {
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        ViewBag.AllExercises = await service.GetAllExercises();
        ViewBag.AllStudents = await service.GetStudents();

        return View(await service.GetVariant(variantId));
    }
    
    [HttpPost]
    public async Task<IActionResult> EditVariant([FromForm] VariantDto updatedVariant)
    {
        if (await service.EditVariant(updatedVariant))
            return RedirectToAction("MyVariants");
        
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        ViewBag.AllExercises = await service.GetMyExercises(userId);
        ViewBag.AllStudents = await service.GetStudents();
        
        return View(updatedVariant);
    }

    [HttpGet]
    public async Task<IActionResult> AddVariant()
    {
        var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        ViewBag.AllExercises = await service.GetMyExercises(userId);
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

    [HttpGet]
    public async Task<IActionResult> MyGroups()
    {
        var teacherId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        
        return View(await service.GetGroups(teacherId));
    }

    [HttpGet]
    public async Task<IActionResult> GetGroup(int groupId)
    {
        return View(await service.GetGroup(groupId));
    }

    [HttpGet]
    public async Task<IActionResult> EditGroup(int groupId)
    {
        var group = await service.GetGroup(groupId);
        var teacherId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        group.TeacherId = teacherId;
        
        ViewBag.Students = await service.GetStudents();
        
        return View(group);
    }

    [HttpPost]
    public async Task<IActionResult> EditGroup([FromForm] EditGroup updatedGroup)
    {
        if (await service.EditGroup(updatedGroup))
            return RedirectToAction(nameof(MyGroups));
        
        ViewBag.Message = "Ошибка редактирования группы.";
        
        return View(await service.GetGroup(updatedGroup.GroupId));
    }

    [HttpGet]
    public async Task<IActionResult> AddGroup()
    {
        ViewBag.TeacherId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        ViewBag.Students = await service.GetStudents();
        
        return View(new AddGroup());
    }

    [HttpPost]
    public async Task<IActionResult> AddGroup([FromForm] AddGroup newGroup)
    {
        if (await service.AddGroup(newGroup))
            return RedirectToAction(nameof(MyGroups));
        
        ViewBag.Message = "Ошибка добавления группы.";
        ViewBag.TeacherId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        ViewBag.Students = await service.GetStudents();

        return View(newGroup);
    }

    [HttpGet]
    public async Task<IActionResult> Students()
    {
        return View(await service.GetStudents());
    }

    [HttpGet]
    public async Task<IActionResult> GetStudent(int studentId)
    {
        return View(await service.GetStudent(studentId));
    }

    [HttpGet]
    public async Task<IActionResult> AllExercises()
    {
        return View(await service.GetAllExercises());
    }

    [HttpGet]
    public async Task<IActionResult> AllVariants()
    {
        return View(await service.GetAllVariants());
    }
    
    [HttpGet]
    public async Task<IActionResult> DeleteVariant(int variantId)
    {
        if (await service.DeleteVariant(variantId))
            return RedirectToAction("MyVariants");

        ViewBag.Message = "Ошибка при удалении варианта.";

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> DeleteExercise(int exerciseId)
    {
        if (await service.DeleteExercise(exerciseId))
            return RedirectToAction("MyExercises");

        ViewBag.Message = "Ошибка при удалении задания.";

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> DeleteGroup(int groupId)
    {
        if (await service.DeleteGroup(groupId))
            return RedirectToAction("MyGroups");

        ViewBag.Message = "Ошибка при удалении задания.";

        return RedirectToAction("Index");
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

    public async Task<IActionResult> GetStudentsSolutions()
    {
        var solutions = await service.GetStudentSolutions();
        
        return View(solutions);
    }
}