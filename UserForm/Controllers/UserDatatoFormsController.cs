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
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private bool IfHasResponse(int formId, string userid)
    {
        return context.FormResponses.Where(fr =>
            fr.FormId == formId && fr.SubmittedById == userid).Any();
    }

    [HttpGet]
    public async Task<IActionResult> Submit(int id)
    {
        if (IfHasResponse(id, GetUserId()))
        {
            var response = context.FormResponses
                .FirstOrDefault(fr => fr.FormId == id && fr.SubmittedById == GetUserId());
            return RedirectToAction("EditSubmission", "MyDashboard", new { responseId = response.Id});
        }
        
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
            Description = form.Description,
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

        var userid = GetUserId();

        var form = await context.Forms
            .Include(f => f.Questions)
            .FirstOrDefaultAsync(f => f.Id == model.FormId);

        if (form == null)
            return NotFound();

        var isduplicate = IfHasResponse(form.Id, userid);
        
        if(isduplicate) return BadRequest();
        
        var response = new FormResponse
        {
            FormId = form.Id,
            SubmittedById = userid, 
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
    
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostComment(
        [FromForm] int FormId,
        [FromForm] string Content,
        [FromForm] string returnUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Content))
            {
                ModelState.AddModelError("Content", "Comment cannot be empty");
            }

            if (!ModelState.IsValid)
            {
                return Redirect(returnUrl);
            }

            var comment = new CommentEntity
            {
                FormId = FormId,
                Content = Content.Trim(),
                UserId = GetUserId(),
                CreatedAt = DateTime.UtcNow
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();

            return Redirect(returnUrl);
        }
        catch (Exception ex)
        {
            return Redirect(returnUrl);
        }
    }
}