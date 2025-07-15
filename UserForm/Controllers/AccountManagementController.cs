using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Users;
using UserForm.Services;
using UserForm.ViewModels.Account;

namespace UserForm.Controllers;
public class AccountController : Controller
{
    private readonly UserManager<UserDetails> _userManager;
    private readonly SignInManager<UserDetails> _signInManager;
    private readonly AppDbContext _db;
    private readonly UserService _userservice;
    private readonly ILogger<AccountController> _logger;

    public AccountController(UserManager<UserDetails> userManager,
    SignInManager<UserDetails> signInManager, UserService userservice, AppDbContext db,
    ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userservice = userservice;
        _db = db;
        _logger = logger;
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
            FullName = model.Name,
            LastLogin = DateTime.UtcNow,
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("List", "FormManage");
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

            return Redirect(returnUrl ?? Url.Action("List", "FormManage")!);
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
    
    [Authorize]
    public IActionResult CreateSalesforceAccount()
    {
        var model = new SalesforceAccountViewModel
        {
            Name = User.Identity.Name,
            UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
        };
        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> CreateSalesforceAccount(SalesforceAccountViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        if (model.UserId != currentUserId && !isAdmin)
            return Forbid();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == model.UserId);
        if (user == null)
        {
            ModelState.AddModelError("", "User not found.");
            return View(model);
        }

        if (!string.IsNullOrEmpty(user.SalesforceAccountId) || !string.IsNullOrEmpty(user.SalesforceContactId))
        {
            ModelState.AddModelError("", "Salesforce record already exists for this user.");
            return View(model);
        }

        try
        {
            var (token, instanceUrl) = await SalesforceHelper.GetAccessTokenAsync();
            var accountId = await SalesforceHelper.CreateAccountAsync(token, instanceUrl, model);
            var contactId = await SalesforceHelper.CreateContactAsync(token, instanceUrl, model, accountId);

            user.SalesforceAccountId = accountId;
            user.SalesforceContactId = contactId;
            await _db.SaveChangesAsync();

            TempData["Message"] = "Salesforce Account and Contact created successfully.";
            return RedirectToAction("Profile", new { id = model.UserId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Salesforce sync failed for user {UserId}", model.UserId);
            ModelState.AddModelError("", $"Error: {ex.Message}");
        }

        return View(model);
    }
}