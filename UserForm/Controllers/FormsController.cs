using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserForm.DTOS;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Forms;
using UserForm.Models.ViewModels;

namespace UserForm.Controllers;

public class FormsController : Controller
{
    private readonly AppDbContext _context;
    public FormsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateFormDto());
    }

    [HttpPost]
    public IActionResult Create(CreateFormDto model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var form = new FormEntity
        {
            FormTitle = model.FormTitle,
            FormTopic = model.FormTopic,
            Tags = model.Tags,
            IsPublic = model.IsPublic,
            Questions = model.Questions.Select(q => new QuestionEntity
            {
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = q.Options?.Select(o => new OptionEntity { Text = o }).ToList() ?? new List<OptionEntity>()
            }).ToList()
        };

        _context.Forms.Add(form);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult CreateFromTemplate(int id)
    {
        var template = _context.Forms
            .Include(f => f.Questions).ThenInclude(q => q.Options)
            .FirstOrDefault(f => f.Id == id && f.IsPublic);

        if (template == null) return NotFound();

        var dto = new CreateFormDto
        {
            FormTitle = template.FormTitle + " (Copy)",
            FormTopic = template.FormTopic,
            Tags = template.Tags,
            IsPublic = false,
            Questions = template.Questions.Select(q => new QuestionDto
            {
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = q.Options?.Select(o => o.Text).ToList()
            }).ToList()
        };

        return View("Create", dto);
    }

    [HttpGet]
    public IActionResult PublicTemplates()
    {
        var templates = _context.Forms
            .Where(f => f.IsPublic)
            .Select(f => new
            {
                f.Id,
                f.FormTitle,
                f.FormTopic,
                f.Tags
            }).ToList();

        return View(templates);
    }

    [HttpGet]
    public IActionResult Submit(int id)
    {
        var form = _context.Forms
            .Include(f => f.Questions).ThenInclude(q => q.Options)
            .FirstOrDefault(f => f.Id == id);

        if (form == null) return NotFound();

        var vm = new SubmitFormViewModel
        {
            FormId = form.Id,
            FormTitle = form.FormTitle,
            FormTopic = form.FormTopic,
            Questions = form.Questions.Select(q => new QuestionViewModel
            {
                QuestionId = q.Id,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = q.Options?.Select(o => o.Text).ToList()
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    public IActionResult Submit(int formId, List<AnswerInputModel> answers)
    {
        foreach (var ans in answers)
        {
            var response = new FormResponse
            {
                QuestionId = ans.QuestionId,
                TextAnswer = ans.TextAnswer,
                NumberAnswer = ans.NumberAnswer,
                SelectedOption = ans.SelectedOption,
                SubmittedAt = DateTime.UtcNow
            };
            _context.FormResponses.Add(response);
        }
        _context.SaveChanges();
        return RedirectToAction("ThankYou");
    }

    [HttpGet]
    public IActionResult Analytics(int id)
    {
        var form = _context.Forms
            .Include(f => f.Questions).ThenInclude(q => q.Options)
            .Include(f => f.Questions).ThenInclude(q => q.Responses)
            .FirstOrDefault(f => f.Id == id);

        if (form == null) return NotFound();

        var vm = new FormAnalyticsViewModel
        {
            FormTitle = form.FormTitle,
            FormTopic = form.FormTopic,
            Questions = form.Questions.Select(q =>
            {
                var analytics = new QuestionAnalyticsModel
                {
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType
                };

                if (q.QuestionType == "Number")
                {
                    var nums = q.Responses.Select(r => r.NumberAnswer).Where(n => n.HasValue).Select(n => n.Value).ToList();
                    analytics.Average = nums.Count > 0 ? nums.Average() : null;
                }
                else if (q.QuestionType == "MultipleChoice")
                {
                    analytics.OptionCounts = q.Options.Select(opt => new OptionStat
                    {
                        Option = opt.Text,
                        Count = q.Responses.Count(r => r.SelectedOption == opt.Text)
                    }).ToList();
                }
                return analytics;
            }).ToList()
        };

        return View(vm);
    }
}