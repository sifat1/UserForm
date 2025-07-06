using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserForm.Models.DBModels.Users;
using UserForm.ViewModels.Admin;

namespace UserForm.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<UserDetails> _userManager;
    private SignInManager<UserDetails> _signInManager;

    public AdminController(UserManager<UserDetails> userManager,SignInManager<UserDetails> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public async Task<IActionResult> Admin()
    {
        var users = _userManager.Users.ToList();
        var model = new AdminUserManagementViewModel();

        foreach (var user in users)
        {
            var userDetails = await _userManager.FindByIdAsync(user.Id);
            var roles = await _userManager.GetRolesAsync(user);
            model.Users.Add(new AdminUserViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                IsAdmin = roles.Contains("Admin"),
                IsLockedOut = await _userManager.IsLockedOutAsync(user),
                IsBlocked = userDetails?.IsBlocked ?? false
            });
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> BulkAction([FromForm] List<string> SelectedUserIds,
        [FromForm] string actionType)
    {
        if (SelectedUserIds == null || !SelectedUserIds.Any())
        {
            TempData["ErrorMessage"] = "No users selected";
            return RedirectToAction("Admin");
        }

        var currentUserEmail = User.Identity?.Name;

        foreach (var userId in SelectedUserIds)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) continue;

            switch (actionType)
            {
                case "block":
                    user.IsBlocked = true;
                    await _userManager.UpdateAsync(user);
                    if (currentUserEmail == user.Email)
                    {
                        await _signInManager.SignOutAsync();
                    }
                    await _userManager.UpdateSecurityStampAsync(user);
                    break;

                case "unblock":
                    user.IsBlocked = false;
                    await _userManager.UpdateAsync(user);
                    break;

                case "makeAdmin":
                    if (!await _userManager.IsInRoleAsync(user, "Admin"))
                        await _userManager.AddToRoleAsync(user, "Admin");
                    break;

                case "removeAdmin":
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                        await _userManager.RemoveFromRoleAsync(user, "Admin");
                    if (currentUserEmail == user.Email)
                    {
                        await _signInManager.SignOutAsync();
                    }
                    await _userManager.UpdateSecurityStampAsync(user);
                    break;

                case "delete":
                    if (currentUserEmail == user.Email)
                    {
                        await _signInManager.SignOutAsync();
                    }
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        TempData["ErrorMessage"] = $"Failed to delete user {user.Email}";
                    }
                    break;
            }
        }

        TempData["SuccessMessage"] = "Bulk action completed successfully";
        return RedirectToAction("Admin");
    }

}
