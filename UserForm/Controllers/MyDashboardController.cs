using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels;
using UserForm.ViewModels.FormManage;

namespace UserForm.Controllers;

[Authorize]
public class MyDashboardController(AppDbContext context) : Controller
{
    
    [HttpGet]
    public async Task<IActionResult> MyForms(int page = 1, int pageSize = 6)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = context.Forms.Where(f => f.CreatedById == userId || User.IsInRole("Admin"));

        int totalCount = await query.CountAsync();
        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var forms = await query
            .OrderByDescending(f => f.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FormCardViewModel
            {
                Id = f.Id,
                FormTitle = f.FormTitle,
                FormTopic = f.FormTopic,
                IsPublic = f.IsPublic
            }).ToListAsync();

        var vm = new UserFormListViewModel
        {
            Forms = forms,
            CurrentPage = page,
            TotalPages = totalPages
        };

        return View(vm);
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSelected(List<int> selectedFormIds)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        foreach (var id in selectedFormIds)
            Console.WriteLine($" - {id}");

        var formsToDelete = await context.Forms
            .Where(f => selectedFormIds.Contains(f.Id) && f.CreatedById == userId)
            .ToListAsync();

        if (formsToDelete.Any())
        {
            context.Forms.RemoveRange(formsToDelete);
            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Form deleted successfully!";
        }

        return RedirectToAction("MyForms");
    }


    [HttpGet]
    public IActionResult Details(int id)
    {
        return View("DetailsTabs", id);
    }

}