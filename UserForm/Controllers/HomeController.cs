using Microsoft.AspNetCore.Mvc;

namespace UserForm.Controllers;


[ApiController]
[Route("api/[controller]")]
public class HomeController : Controller
{
    [HttpGet("/")]
    public void Index()
    {
        
    }
}