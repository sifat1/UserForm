using FormGenerator.Models.DBModels.Question;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserForm.DTOS;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Forms;
using UserForm.Models.DBModels.Users;
using UserForm.Models.ViewModels;

namespace UserForm.Controllers;

public class FormsController(AppDbContext context,UserManager<UserDetails> userManager, ILogger<FormsController> logger) : Controller
{
    [HttpGet]
    public ViewResult CreateForm()
    {
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> CreateForm(CreateFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state while creating form.");
            return View(model);
        }

        var baseForm = new BaseForm
        {
            Title = model.FormTitle,
            Questions = model.Questions.Select(q => CreateQuestion(q)).ToList()
        };

        context.Forms.Add(baseForm);
        var userform = new UserForms
        {
            FormTemplateId = baseForm.Id,
            FormownerId = userManager.GetUserId(User),
        };
        context.UserForms.Add(userform);
        await context.SaveChangesAsync();

        logger.LogInformation("Form '{FormTitle}' created with {QuestionCount} questions.", baseForm.Title, baseForm.Questions.Count);
        return RedirectToAction("Index");
    }

    private BaseQuestion CreateQuestion(QuestionInputViewModel q)
    {
        return q.QuestionType switch
        {
            "Text" => new QuestionwithTextOption
            {
                Questiontxt = q.QuestionText,
                TextAnswer = ""
            },
            "Options" => new QuestionwithOptions
            {
                Questiontxt = q.QuestionText,
                OptionAnswer = q.Options != null && q.CorrectOptionIndex is int idx && idx < q.Options.Count
                    ? q.Options[idx]
                    : null,
                Options = q.Options?.Select(opt => new Options { OptionText = opt }).ToList() ?? new List<Options>()
            },
            _ => throw new InvalidOperationException($"Unsupported question type: {q.QuestionType}")
        };
    }

    public async Task<ActionResult> UpdateForm(CreateFormDto model)
    {
        if (!ModelState.IsValid) return Redirect("/");
        var form = new BaseForm { Title = model.Title, Questions = model.Questions };
        context.Update(form);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<ActionResult> DeleteForm(CreateFormDto model)
    {
        context.Remove(model);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<ActionResult> FormAnalatics(int id)
    {
        var form = context.UserForms.Where(f => f.Id == id);
        return Ok();
    }
}