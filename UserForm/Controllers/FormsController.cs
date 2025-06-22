using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels;
using UserForm.DTOS;
using UserForm.Models.DBModels.Forms;
using UserForm.Models.DBModels.Question;
using UserForm.Models.ViewModels;

public class FormsController : Controller
{
    private readonly AppDbContext _context;

    public FormsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Create form page
    [HttpGet]
    public IActionResult Create()
    {
        var model = new CreateFormDto
        {
            FormTitle = "Test Form",
            FormTopic = "Science",
            Tags = "sample,tag",
            IsPublic = true,
            Questions = new List<QuestionDto>()
        };

        return View(model);
    }


    // POST: Save new form
    [HttpPost]
    public async Task<IActionResult> Create(CreateFormDto dto)
    {
        if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"[Model Error] {state.Key}: {error.ErrorMessage}");
                    }
                }
                return View(dto);
            }


        var formEntity = new FormEntity
        {
            FormTitle = dto.FormTitle,
            FormTopic = dto.FormTopic,
            Tags = dto.Tags,
            IsPublic = dto.IsPublic,
            Questions = dto.Questions.Select(q => new QuestionEntity
            {
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = q.QuestionType == "MultipleChoice" 
                    ? q.Options.Select(opt => new OptionEntity { OptionText = opt }).ToList() 
                    : new List<OptionEntity>()
            }).ToList()
        };

        _context.Forms.Add(formEntity);
        await _context.SaveChangesAsync();

        return RedirectToAction("Edit", new { id = formEntity.Id });
    }

    // GET: Edit form page
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var form = await _context.Forms
            .Include(f => f.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (form == null) return NotFound();

        var model = new CreateFormDto
        {
            Id = form.Id,
            FormTitle = form.FormTitle,
            FormTopic = form.FormTopic,
            Tags = form.Tags,
            IsPublic = form.IsPublic,
            Questions = form.Questions.Select(q => new QuestionDto
            {
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = q.Options.Select(o => o.OptionText).ToList()
            }).ToList()
        };

        return View(model);
    }

    // POST: Save edited form
    [HttpPost]
    public async Task<IActionResult> Edit(CreateFormDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var form = await _context.Forms
            .Include(f => f.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(f => f.Id == dto.Id);

        if (form == null) return NotFound();

        // Update form properties
        form.FormTitle = dto.FormTitle;
        form.FormTopic = dto.FormTopic;
        form.Tags = dto.Tags;
        form.IsPublic = dto.IsPublic;

        // Remove all existing questions/options (simplest approach)
        _context.Options.RemoveRange(form.Questions.SelectMany(q => q.Options));
        _context.Questions.RemoveRange(form.Questions);

        // Add updated questions/options
        form.Questions = dto.Questions.Select(q => new QuestionEntity
        {
            QuestionText = q.QuestionText,
            QuestionType = q.QuestionType,
            Options = q.QuestionType == "MultipleChoice"
                ? q.Options.Select(opt => new OptionEntity { OptionText = opt }).ToList()
                : new List<OptionEntity>()
        }).ToList();

        await _context.SaveChangesAsync();

        return RedirectToAction("Edit", new { id = form.Id });
    }


    [HttpGet]
    public async Task<IActionResult> CreateFromTemplate(int id)
    {
        var form = await _context.Forms
            .Include(f => f.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (form == null) return NotFound();

        var model = new CreateFormDto
        {
            Id = form.Id,
            FormTitle = form.FormTitle,
            FormTopic = form.FormTopic,
            Tags = form.Tags,
            IsPublic = form.IsPublic,
            Questions = form.Questions.Select(q => new QuestionDto
            {
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = q.Options.Select(o => o.OptionText).ToList()
            }).ToList()
        };
        return View(model);
    }


    // GET: Show analytics for a form
    [HttpGet]
    public async Task<IActionResult> Analytics(int id)
    {
        var form = await _context.Forms
            .Include(f => f.Questions)
                .ThenInclude(q => q.Options)
            .Include(f => f.Responses)
                .ThenInclude(r => r.Answers)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (form == null) return NotFound();

        var analytics = new FormAnalyticsDto
        {
            FormId = form.Id,
            FormTitle = form.FormTitle
        };

        foreach (var q in form.Questions)
        {
            var qAnalytics = new QuestionAnalyticsDto
            {
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
            };

            if (q.QuestionType == "Number")
            {
                var numberAnswers = form.Responses
                    .SelectMany(r => r.Answers)
                    .Where(a => a.QuestionId == q.Id && double.TryParse(a.AnswerText, out _))
                    .Select(a => double.Parse(a.AnswerText));

                if (numberAnswers.Any())
                    qAnalytics.AverageNumberAnswer = numberAnswers.Average();
            }
            else if (q.QuestionType == "MultipleChoice")
            {
                var optionCounts = new Dictionary<string, int>();

                foreach (var option in q.Options)
                    optionCounts[option.OptionText] = 0;

                var answers = form.Responses
                    .SelectMany(r => r.Answers)
                    .Where(a => a.QuestionId == q.Id);

                foreach (var ans in answers)
                {
                    if (optionCounts.ContainsKey(ans.AnswerText))
                        optionCounts[ans.AnswerText]++;
                }

                qAnalytics.OptionFrequencies = optionCounts;
            }

            analytics.Questions.Add(qAnalytics);
        }

        return View(analytics);
    }
}
