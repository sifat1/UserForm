using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Forms;
using UserForm.Models.DBModels.Users;
using UserForm.ViewModels.FormManage;
using UserForm.ViewModels.Mydashboard;
using UserForm.ViewModels.usersubmitformdata;

namespace UserForm.Controllers;

[Authorize]
public class MyDashboardController(AppDbContext context,UserManager<UserDetails> _userManager) : Controller
{
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
    
    [HttpGet]
    public async Task<IActionResult> MyForms(int page = 1)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var query = context.Forms
            .Where(f => f.CreatedById == userId || User.IsInRole("Admin"))
            .Select(f => new FormCardViewModel
            {
                Id = f.Id,
                FormTitle = f.FormTitle,
                FormTopic = f.FormTopic,
                IsPublic = f.IsPublic
            })
            .OrderByDescending(f => f.Id); // 👈 Now this is on FormCardViewModel

        var pagingList = await PagingList.CreateAsync(query, 6, page);

        var vm = new UserFormListViewModel
        {
            Forms = pagingList.ToList(),
            CurrentPage = pagingList.PageIndex,
            TotalPages = pagingList.PageCount
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
            .Where(f => selectedFormIds.Contains(f.Id) && f.CreatedById == userId || User.IsInRole("Admin"))
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

    [HttpGet]
    public async Task<IActionResult> FormSubmissions(int formId, int page = 1)
    {
        var form = await context.Forms.FindAsync(formId);
        if (form == null) return NotFound();
        if (form.CreatedById == GetUserId() || User.IsInRole("Admin"))
        {
            var questions = await context.Questions
                .Where(q => q.FormId == formId)
                .Include(q => q.Options)
                .ToListAsync();

            var query = context.FormResponses
                .Where(r => r.FormId == formId)
                .Include(r => r.Answers)
                .Include(r => r.SubmittedBy)
                .OrderByDescending(r => r.SubmittedAt);

            var model = await PagingList.CreateAsync(query, 5, page);

            model.RouteValue = new RouteValueDictionary
            {
                { "formId", formId }
            };

            ViewBag.Questions = questions;
            ViewBag.FormTitle = form.FormTitle;
            ViewBag.FormId = formId;

            return View(model);
        }

        return Unauthorized();
    }


    
    [HttpGet]
    public async Task<IActionResult> EditSubmission(int responseId)
    {
        var response = await context.FormResponses
            .Include(r => r.Answers)
            .Include(r => r.Form)
            .ThenInclude(f => f.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(r => r.Id == responseId);

        if (response == null)
            return NotFound();
        
        if (response.Form.CreatedById == GetUserId()|| response.SubmittedById == GetUserId() || User.IsInRole("Admin"))
        {
        var vm = new SubmitFormViewModel
        {
            FormId = response.FormId,
            ResponseId = response.Id,
            FormTitle = response.Form.FormTitle,
            FormTopic = response.Form.FormTopic,

            Questions = response.Form.Questions.Select(q => new QuestionViewModel
            {
                QuestionId = q.Id,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = q.Options.Select(o => o.OptionText).ToList()
            }).ToList(),

            Answers = response.Form.Questions.Select(q =>
            {
                var a = response.Answers.FirstOrDefault(ans => ans.QuestionId == q.Id);
                return new AnswerInputModel
                {
                    QuestionId = q.Id,
                    TextAnswer = a?.AnswerText 
                };
            }).ToList()

        };

        return View("EditSubmission", vm);
        }

        return Unauthorized();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSubmission(SubmitFormViewModel vm)
    {
        var response = await context.FormResponses
            .Include(r => r.Answers)
            .FirstOrDefaultAsync(r => r.FormId == vm.FormId && r.Id == vm.ResponseId); 

        if (response == null) return NotFound();
        if (response.Form.CreatedById == GetUserId()|| response.SubmittedById == GetUserId() || User.IsInRole("Admin"))
        {
        foreach (var answer in vm.Answers)
        {
            var existingAnswer = response.Answers.FirstOrDefault(a => a.QuestionId == answer.QuestionId);
            if (existingAnswer != null)
            {
                existingAnswer.AnswerText = answer.TextAnswer?.Trim();
            }
        }

        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Submission updated successfully!";
        return RedirectToAction("EditSubmission", new { responseId = vm.ResponseId });
        }

        return Unauthorized();
    }
    
    [HttpGet]
    public async Task<IActionResult> ManageFormAccess(int formId)
    {
        var form = await context.Forms
            .Include(f => f.SharedWithUsers)
            .ThenInclude(fa => fa.User)
            .FirstOrDefaultAsync(f => f.Id == formId);

        var model = new FormAccessManageViewModel
        {
            FormId = formId,
            AccessUserEmails = form.SharedWithUsers.Select(u => u.User.Email).ToList()
        };

        return View(model);
    }
    
    [HttpPost]
    public async Task<IActionResult> ShareForm(int formId, string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("ManageFormAccess", new { formId });
        }

        var exists = await context.FormAccess
            .AnyAsync(fa => fa.FormId == formId && fa.UserId == user.Id);

        if (!exists)
        {
            context.FormAccess.Add(new FormAccess { FormId = formId, UserId = user.Id });
            await context.SaveChangesAsync();
        }

        return RedirectToAction("ManageFormAccess", new { formId });
    }
    
    [HttpPost]
    public async Task<IActionResult> RemoveSharedUsers(int formId, List<string> selectedEmails)
    {
        if (selectedEmails == null || !selectedEmails.Any())
        {
            TempData["ErrorMessage"] = "No users selected for removal.";
            return RedirectToAction("ManageFormAccess", new { formId });
        }

        var users = await _userManager.Users
            .Where(u => selectedEmails.Contains(u.Email))
            .ToListAsync();

        var accessEntries = await context.FormAccess
            .Where(fa => fa.FormId == formId && users.Select(u => u.Id).Contains(fa.UserId))
            .ToListAsync();

        context.FormAccess.RemoveRange(accessEntries);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Selected users removed from access.";
        return RedirectToAction("ManageFormAccess", new { formId });
    }

}
