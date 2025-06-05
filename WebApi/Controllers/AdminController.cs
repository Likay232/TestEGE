using Microsoft.AspNetCore.Mvc;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Requests;
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

    /*
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        try
        {
            var users = await service.GetUsers();

            return StatusCode(200, users);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<bool>> ChangeUserPassword(ChangePassword request)
    {
        try
        {
            return StatusCode(200, await service.ChangeUserPassword(request));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<ThemeDto>>> GetThemes()
    {
        try
        {
            var themes = await service.GetThemes();

            return StatusCode(200, themes);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<bool>> CreateNewTheme(CreateTheme request)
    {
        try
        {
            return StatusCode(200, await service.CreateNewTheme(request));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<bool>> AddTaskForTheme(TaskDto taskToAdd)
    {
        try
        {
            return StatusCode(200, await service.AddTaskForTheme(taskToAdd));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<bool>> EditTaskForTheme(TaskDto updatedEntry)
    {
        try
        {
            return StatusCode(200, await service.EditTaskForTheme(updatedEntry));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<bool>> DeleteTaskForTheme(int taskId)
    {
        try
        {
            return StatusCode(200, await service.DeleteTaskForTheme(taskId));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<bool>> AddLessonForTheme(LessonDto lessonToAdd)
    {
        try
        {
            return StatusCode(200, await service.AddLessonForTheme(lessonToAdd));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<TaskDto>>> GetTasks()
    {
        try
        {
            var tasks = await service.GetTasks();

            return StatusCode(200, tasks);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreateTest(CreateTest request)
    {
        try
        {
            return StatusCode(200, await service.CreateTest(request));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<bool>> SwitchBlockState(int userId)
    {
        try
        {
            return StatusCode(200, await service.SwitchBlockState(userId));
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    */
}