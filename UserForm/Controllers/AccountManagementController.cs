using Microsoft.AspNetCore.Mvc;

namespace UserForm.Controllers;

class AccountManagementController : Controller
{
    [HttpGet("/Login")]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost("/Login")]
    public IActionResult Login([FromForm] object loginDetails)
    {
        // Process login details here
        return RedirectToAction("Index", "Home");
    }

}