using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Forms;
using UserForm.ViewModels.FormManage;
using UserForm.ViewModels.usersubmitformdata;

namespace UserForm.Controllers;

[Authorize]
public class MyDashboardController(AppDbContext context) : Controller
{
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

    [HttpGet]
    public async Task<IActionResult> FormSubmissions(int formId, int page = 1)
    {
        var form = await context.Forms.FindAsync(formId);
        if (form == null) return NotFound();

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

        model.RouteValue = new RouteValueDictionary {
            { "formId", formId }
        };

        ViewBag.Questions = questions;
        ViewBag.FormTitle = form.FormTitle;
        ViewBag.FormId = formId;

        return View(model);
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
                    TextAnswer = a?.AnswerText // used for all question types
                };
            }).ToList()

        };

        return View("EditSubmission", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSubmission(SubmitFormViewModel vm)
    {
        var response = await context.FormResponses
            .Include(r => r.Answers)
            .FirstOrDefaultAsync(r => r.FormId == vm.FormId && r.Id == vm.ResponseId); // or pass responseId as hidden field

        if (response == null) return NotFound();

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

}
