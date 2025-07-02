using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserForm.Models.DBModels.Users;
using UserForm.ViewModels.Admin;

namespace UserForm.Controllers;


[Authorize(Roles = "Admin")]
public class AdminController(UserManager<UserDetails> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Admin()
    {
        var users = userManager.Users.ToList();
        var model = new AdminUserManagementViewModel();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            model.Users.Add(new AdminUserViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                IsAdmin = roles.Contains("Admin"),
                IsLockedOut = await userManager.IsLockedOutAsync(user)
            });
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> BulkAction(
        [FromForm]List<string> SelectedUserIds, 
        [FromForm] string actionType)
    {
        if (SelectedUserIds == null || !SelectedUserIds.Any())
        {
            TempData["ErrorMessage"] = "No users selected";
            return RedirectToAction("Admin");
        }

        foreach (var userId in SelectedUserIds)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) continue;

            switch (actionType)
            {
                case "block":
                    user.IsBlocked = true;
                    await userManager.UpdateAsync(user);
                    break;
                case "unblock":
                    user.IsBlocked = false;
                    await userManager.UpdateAsync(user);
                    break;
                case "makeAdmin":
                    if (!await userManager.IsInRoleAsync(user, "Admin"))
                        await userManager.AddToRoleAsync(user, "Admin");
                    break;
                case "removeAdmin":
                    if (await userManager.IsInRoleAsync(user, "Admin"))
                        await userManager.RemoveFromRoleAsync(user, "Admin");
                    break;
            }
        }

        TempData["SuccessMessage"] = "Bulk action completed successfully";
        return RedirectToAction("Admin");
    }
}