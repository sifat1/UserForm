using Microsoft.AspNetCore.Mvc;

namespace UserForm.Controllers;


public class FormsController : Controller
{
    [HttpGet("/Forms")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("/Forms")]
    public IActionResult Create([FromForm] object name)
    {
        
        return View();
    }
}