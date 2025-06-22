using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Users;
using UserForm.Models.ViewModels;

namespace UserForm.Controllers;
public class AccountController : Controller
{
    private readonly UserManager<UserDetails> _userManager;
    private readonly SignInManager<UserDetails> _signInManager;
    private readonly AppDbContext _db;
    private readonly UserService _userservice;

    public AccountController(UserManager<UserDetails> userManager,
    SignInManager<UserDetails> signInManager, UserService userservice, AppDbContext db,
    ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userservice = userservice;
        _db = db;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Submitted Empty/Invalid form.");
            return View(model);
        }

        if (_userservice.IfUserExists(model.Email))
        {
            ModelState.AddModelError(string.Empty, "User with this Email already exists.");
            return View(model);
        }

        var user = new UserDetails
        {
            Email = model.Email,
            IsBlocked = false,
            UserName = model.Email,
            LastLogin = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("ShowUsers", "ManageUser");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if(!ModelState.IsValid) return View(model);

        if(!_userservice.IfUserExists(model.Email))
        {
            ModelState.AddModelError(string.Empty,"User doesn't exsist / been blocked");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            await _db.Users.Where(u => u.Email == model.Email).ExecuteUpdateAsync(setters => 
                    setters.SetProperty(u => u.LastLogin, DateTime.UtcNow));

            return Redirect(returnUrl ?? Url.Action("ShowUsers", "ManageUser")!);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt");
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }
}