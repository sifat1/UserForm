using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserForm.DTOS;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Forms;
using UserForm.Models.DBModels.Question;
using UserForm.ViewModels.Analytics;
using UserForm.ViewModels.usersubmitformdata;

namespace UserForm.Controllers;


public class FormsController(AppDbContext context) : Controller
{
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private bool HasTooManyOfSameQuestionType(List<QuestionDto> questions, out string errorMessage)
    {
        var typeGroups = questions
            .GroupBy(q => q.QuestionType)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var (type, count) in typeGroups)
        {
            if (count > 4)
            {
                errorMessage = $"You cannot have more than 4 questions of type '{type}'.";
                return true;
            }
        }

        errorMessage = "";
        return false;
    }

    private List<string> GetTopics() =>
        context.Topics.Select(f => f.TopicName).Distinct().ToList();

    private FormEntity MapDtoToEntity(CreateFormDto dto) => new()
    {
        FormTitle = dto.FormTitle,
        Description = dto.FormDescription,
        FormTopic = dto.FormTopic,
        Tags = dto.Tags,
        IsPublic = dto.IsPublic,
        CreatedById = GetUserId(),
        Questions = dto.Questions.Select(q => new QuestionEntity
        {
            QuestionText = q.QuestionText,
            QuestionType = q.QuestionType,
            Options = q.QuestionType == "MultipleChoice"
                ? q.Options.Select(opt => new OptionEntity { OptionText = opt }).ToList()
                : new List<OptionEntity>()
        }).ToList()
    };

    private CreateFormDto MapEntityToDto(FormEntity form) => new()
    {
        Id = form.Id,
        FormTitle = form.FormTitle,
        FormDescription = form.Description,
        FormTopic = form.FormTopic,
        Tags = form.Tags,
        IsPublic = form.IsPublic,
        Topics = GetTopics(),
        Questions = form.Questions.Select(q => new QuestionDto
        {
            QuestionText = q.QuestionText,
            QuestionType = q.QuestionType,
            Options = q.Options.Select(o => o.OptionText).ToList()
        }).ToList()
    };

    private async Task<FormEntity?> GetFormWithDetailsAsync(int id) =>
        await context.Forms
            .Include(f => f.Questions).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(f => f.Id == id);

    private bool UserOwnsForm(FormEntity form) =>
        form.CreatedById == GetUserId();

    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        return View(new CreateFormDto
        {
            FormTitle = "Test Form",
            FormTopic = "Science",
            Tags = "sample,tag",
            IsPublic = true,
            Questions = new List<QuestionDto>(),
            Topics = GetTopics()
        });
    }

    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CreateFormDto dto)
    {
        if (HasTooManyOfSameQuestionType(dto.Questions, out var errorMessage))
        {
            TempData["ErrorMessage"] = errorMessage;
            dto.Topics = GetTopics();
            return View(dto);
        }

        if (!ModelState.IsValid)
        {
            dto.Topics = GetTopics();
            return View(dto);
        }

        context.Forms.Add(MapDtoToEntity(dto));
        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Form created successfully!";
        return RedirectToAction("MyForms", "MyDashboard");
    }


    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var form = await GetFormWithDetailsAsync(id);
        if (form == null) return NotFound();

        return User.IsInRole("Admin") || UserOwnsForm(form) ? View(MapEntityToDto(form)): Unauthorized();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Edit(CreateFormDto dto)
    {
        if (HasTooManyOfSameQuestionType(dto.Questions, out var errorMessage))
        {
            TempData["ErrorMessage"] = errorMessage;
            dto.Topics = GetTopics();
            return View(dto);
        }

        if (!ModelState.IsValid)
        {
            dto.Topics = GetTopics();
            return View(dto);
        }

        var form = await GetFormWithDetailsAsync(dto.Id ?? 0);
        if (form != null && (User.IsInRole("Admin") || UserOwnsForm(form)))
        {
            form.FormTitle = dto.FormTitle;
            form.Description = dto.FormDescription;
            form.FormTopic = dto.FormTopic;
            form.Tags = dto.Tags;
            form.IsPublic = dto.IsPublic;

            context.Options.RemoveRange(form.Questions.SelectMany(q => q.Options));
            context.Questions.RemoveRange(form.Questions);

            form.Questions = MapDtoToEntity(dto).Questions;

            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Form edited successfully!";
            return RedirectToAction("Edit", new { id = form.Id });
        }

        return Unauthorized();
    }


    [HttpGet]
    public async Task<IActionResult> CreateFromTemplate(int id)
    {
        var form = await GetFormWithDetailsAsync(id);
        if (form == null) return NotFound();
        
        ViewData["Comments"] = await context.Comments
            .Where(c => c.FormId == id)
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommentDisplayViewModel
            {
                Email = c.User.Email,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
        
        return View(MapEntityToDto(form));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateFromTemplate(CreateFormDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        context.Forms.Add(MapDtoToEntity(dto));
        await context.SaveChangesAsync();

        var savedId = context.Forms.OrderByDescending(f => f.Id).First().Id;
        TempData["SuccessMessage"] = "Form created successfully!";
        return RedirectToAction("MyForms", "MyDashboard");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Analytics(int id)
    {
        var form = await context.Forms
            .Include(f => f.Questions).ThenInclude(q => q.Options)
            .Include(f => f.Responses).ThenInclude(r => r.Answers)
            .FirstOrDefaultAsync(f => f.Id == id && f.CreatedById == GetUserId());

        if (form == null) return NotFound();

        var vm = new FormAnalyticsViewModel
        {
            FormId = form.Id,
            FormTitle = form.FormTitle,
            FormTopic = form.FormTopic,
            TotalResponses = form.Responses.Count
        };

        foreach (var question in form.Questions)
        {
            var analytics = new QuestionAnalyticsModel
            {
                QuestionId = question.Id,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType
            };

            var answers = form.Responses.SelectMany(r => r.Answers)
                .Where(a => a.QuestionId == question.Id)
                .ToList();

            if (question.QuestionType == "Number")
            {
                var nums = answers
                    .Where(a => double.TryParse(a.AnswerText, out _))
                    .Select(a => double.Parse(a.AnswerText))
                    .ToList();

                analytics.AverageNumberAnswer = nums.Any() ? nums.Average() : null;
            }
            else if (question.QuestionType == "MultipleChoice")
            {
                var freq = question.Options.ToDictionary(o => o.OptionText, _ => 0);
                foreach (var ans in answers)
                {
                    if (ans.AnswerText != null && freq.ContainsKey(ans.AnswerText))
                        freq[ans.AnswerText]++;
                }
                analytics.OptionFrequency = freq;
            }

            vm.Questions.Add(analytics);
        }

        return View(vm);
    }

}