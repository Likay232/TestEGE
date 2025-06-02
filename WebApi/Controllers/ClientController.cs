using Microsoft.AspNetCore.Mvc;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ClientController(ClientService service) : ControllerBase
{
    /*
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
    public async Task<ActionResult<List<TaskForClientDto>>> GetTasksForTheme(GetTasks request)
    {
        try
        {
            var tasks = await service.GetTasksForTheme(request);
            
            return StatusCode(200, tasks);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<List<LessonDto>>> GetLessonsForTheme(int themeId)
    {
        try
        {
            var lessons = await service.GetLessonsForTheme(themeId);
            
            return StatusCode(200, lessons);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<TestDto>>> GetTests()
    {
        try
        {
            var tests = await service.GetTests();
            
            return StatusCode(200, tests);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    
    [HttpGet]
    public async Task<ActionResult<List<TaskForClientDto>>> GetTest(int testId)
    {
        try
        {
            var tasks = await service.GetTest(testId);
            
            return StatusCode(200, tasks);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<List<TaskForClientDto>>> CheckTest(TestForCheck request)
    {
        try
        {
            var checkedTest = await service.CheckTest(request);
            
            return StatusCode(200, checkedTest);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
    */


}