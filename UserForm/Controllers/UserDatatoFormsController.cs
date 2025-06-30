using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Forms;
using UserForm.Models.DBModels.Question;
using UserForm.ViewModels.usersubmitformdata;

namespace UserForm.Controllers;



public class UserDatatoFormsController(AppDbContext context) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Submit(int id)
    {
        var form = await context.Forms
            .Include(f => f.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (form == null)
            return NotFound();

        var model = new SubmitFormViewModel
        {
            FormId = form.Id,
            FormTitle = form.FormTitle,
            FormTopic = form.FormTopic,
            Questions = form.Questions.Select(q => new QuestionViewModel
            {
                QuestionId = q.Id,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = q.Options?.Select(o => o.OptionText).ToList()
            }).ToList(),
            Answers = form.Questions.Select(q => new AnswerInputModel
            {
                QuestionId = q.Id
            }).ToList() 
        };

        return View(model);
    }


    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState); 
        }

        var form = await context.Forms
            .Include(f => f.Questions)
            .FirstOrDefaultAsync(f => f.Id == model.FormId);

        if (form == null)
            return NotFound();

        var response = new FormResponse
        {
            FormId = form.Id,
            SubmittedById = User.FindFirstValue(ClaimTypes.NameIdentifier), 
            SubmittedAt = DateTime.UtcNow,
            Answers = model.Answers.Select(a => new AnswerEntity
            {
                QuestionId = a.QuestionId,
                AnswerText = a.TextAnswer ?? a.SelectedOption ?? a.NumberAnswer?.ToString()
            }).ToList()
        };

        context.FormResponses.Add(response);
        await context.SaveChangesAsync();

        return RedirectToAction("List", "FormManage"); 
    }

    
}