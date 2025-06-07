using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class GuestController(GuestService service) : Controller
{
    [HttpGet]
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
    public async Task<IActionResult> GetVariant(int variantId)
    {
        return View(await service.GetVariant(variantId));
    }

}