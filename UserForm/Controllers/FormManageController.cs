using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Forms;
using UserForm.ViewModels.FormManage;

namespace UserForm.Controllers;

public class FormManageController(AppDbContext context) : Controller
{

    [HttpGet("/")]
    public async Task<IActionResult> List(string? topic, string? tag, string? searchQuery, int page = 1, int pageSize = 6)
    {
        var topTemplates = await GetTopTemplatesAsync();
        ViewData["TopTemplates"] = topTemplates;

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = context.Forms.AsQueryable();
        if (!string.IsNullOrEmpty(topic))
            query = query.Where(f => f.FormTopic == topic);

        if (!string.IsNullOrEmpty(tag))
            query = query.Where(f => f.Tags.Contains(tag));

        if (!string.IsNullOrEmpty(searchQuery))
        {
            var tsQuery = EF.Functions.PlainToTsQuery("english", searchQuery);
    
            query = query
                .Where(f => f.FormSearchVector.Matches(tsQuery))
                .OrderByDescending(f => EF.Functions.ToTsVector("english", f.FormTitle)
                    .Rank(EF.Functions.ToTsQuery("english", searchQuery)));
        }

        
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
                IsPublic = f.IsPublic,
                LikeCount = context.Likes.Count(l => l.FormId == f.Id)
            }).ToListAsync();

        // Topics and Tags
        var allTopics = await context.Forms
            .Select(f => f.FormTopic)
            .Distinct()
            .ToListAsync();

        var allTagStrings = await context.Forms
            .Select(f => f.Tags)
            .ToListAsync();

        var allTags = allTagStrings
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .SelectMany(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(t => t.Trim())
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        
        var vm = new PaginatedFormListViewModel
        {
            Forms = forms,
            CurrentPage = page,
            TotalPages = totalPages,
            CurrentUserId = currentUserId,
            SelectedTopic = topic,
            SelectedTag = tag,
            SearchQuery = searchQuery,
            AvailableTopics = allTopics,
            AvailableTags = allTags
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Like(int formId, string? returnUrl)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var alreadyLiked = await context.Likes.AnyAsync(l => l.FormId == formId && l.UserId == userId);

        if (!alreadyLiked)
        {
            context.Likes.Add(new LikeEntity { FormId = formId, UserId = userId });
            await context.SaveChangesAsync();
        }

        return !string.IsNullOrEmpty(returnUrl) ? Redirect(returnUrl) : RedirectToAction("List");
    }
    
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Comment(int formId, string content)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(content))
        {
            var comment = new CommentEntity
            {
                FormId = formId,
                UserId = userId,
                Content = content
            };
            context.Comments.Add(comment);
            await context.SaveChangesAsync();
        }
        return RedirectToAction("List"); 
    }
    
    public async Task<List<FormCardViewModel>> GetTopTemplatesAsync()
    {
        return await context.FormResponses
            .GroupBy(r => r.FormId)
            .Select(g => new
            {
                FormId = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(g => g.Count)
            .Take(5)
            .Join(context.Forms,
                r => r.FormId,
                f => f.Id,
                (r, f) => new FormCardViewModel
                {
                    Id = f.Id,
                    FormTitle = f.FormTitle,
                    FormTopic = f.FormTopic,
                    IsPublic = f.IsPublic,
                    LikeCount = context.Likes.Count(l => l.FormId == f.Id)
                })
            .ToListAsync();
    }
    

    [HttpPost]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        Console.WriteLine($"Culture selected: {culture}");
        Console.WriteLine($"Return URL: {returnUrl}");

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );

        return LocalRedirect(returnUrl ?? "/");
    }

}