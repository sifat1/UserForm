using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Forms;
using UserForm.Models.DBModels.Question;
using UserForm.ViewModels.usersubmitformdata;

namespace UserForm.Controllers;


public class UserDatatoFormsController : Controller
{
    private readonly AppDbContext _context;

    public UserDatatoFormsController(AppDbContext context)
    {
        _context = context;
    }
    // GET: Forms/Submit/5
    [HttpGet]
    public async Task<IActionResult> Submit(int id)
    {
        var form = await _context.Forms
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
            }).ToList() // Important!
        };

        return View(model);
    }


[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Submit(SubmitFormViewModel model)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState); // Return detailed model errors
    }

    var form = await _context.Forms
        .Include(f => f.Questions)
        .FirstOrDefaultAsync(f => f.Id == model.FormId);

    if (form == null)
        return NotFound();

    var response = new FormResponse
    {
        FormId = form.Id,
        SubmittedAt = DateTime.UtcNow,
        Answers = model.Answers.Select(a => new AnswerEntity
        {
            QuestionId = a.QuestionId,
            AnswerText = a.TextAnswer ?? a.SelectedOption ?? a.NumberAnswer?.ToString()
        }).ToList()
    };

    _context.FormResponses.Add(response);
    await _context.SaveChangesAsync();

    return RedirectToAction("ThankYou");
}


    public IActionResult ThankYou()
    {
        return View(); // Show confirmation
    }

    
}