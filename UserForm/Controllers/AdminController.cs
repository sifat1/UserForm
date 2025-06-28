using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserForm.Models.DBModels.Users;
using UserForm.ViewModels.Admin;

namespace UserForm.Controllers;

public class AdminController(UserManager<UserDetails> _userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Admin()
    {
        var users = _userManager.Users.ToList();
        var model = new AdminUserManagementViewModel();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            model.Users.Add(new AdminUserViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                IsAdmin = roles.Contains("Admin"),
                IsLockedOut = await _userManager.IsLockedOutAsync(user)
            });
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> BulkAction(AdminUserManagementViewModel vm, string actionType)
    {
        foreach (var userId in vm.SelectedUserIds ?? new List<string>())
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) continue;

            switch (actionType)
            {
                case "block":
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                    break;
                case "unblock":
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                    break;
                case "makeAdmin":
                    if (!await _userManager.IsInRoleAsync(user, "Admin"))
                        await _userManager.AddToRoleAsync(user, "Admin");
                    break;
                case "removeAdmin":
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                        await _userManager.RemoveFromRoleAsync(user, "Admin");
                    break;
            }
        }

        return RedirectToAction("Admin");
    }

}