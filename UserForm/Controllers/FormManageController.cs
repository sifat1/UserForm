using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Forms;
using UserForm.ViewModels.FormManage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

namespace UserForm.Controllers;

public class FormManageController(AppDbContext context) : Controller
{
    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private static bool CheckUserinList(AppDbContext context, string userEmail, int formId)
        => context.FormAccess.Any(u => u.FormId == formId && u.UserId == userEmail);
    
   [HttpGet("/")]
    public async Task<IActionResult> List(string? tag, string? searchQuery, int page = 1, int pageSize = 6)
    {
        var topTemplates = await GetTopTemplatesAsync();
        ViewData["TopTemplates"] = topTemplates;

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        IQueryable<FormEntity> query = context.Forms;

        
        if (!string.IsNullOrEmpty(tag))
            query = query.Where(f => f.Tags.Contains(tag) || 
                                   f.Tags.StartsWith(tag + ",") || 
                                   f.Tags.EndsWith("," + tag) || 
                                   f.Tags.Contains("," + tag + ","));

        
        if (!string.IsNullOrEmpty(searchQuery))
        {
            var formIds = await context.Database
                .SqlQuery<int>($"""
                                    WITH search_results AS (
                                        SELECT 
                                            f."Id",
                                            ts_rank(f."FormSearchVector", plainto_tsquery('english', {searchQuery})) AS form_rank,
                                            COALESCE((
                                                SELECT SUM(ts_rank(c."CommentSearchVector", plainto_tsquery('english', {searchQuery})))
                                                FROM "Comments" c
                                                WHERE c."FormId" = f."Id"
                                            ), 0) AS comment_rank
                                        FROM "Forms" f
                                        WHERE 
                                            f."FormSearchVector" @@ plainto_tsquery('english', {searchQuery})
                                            OR EXISTS (
                                                SELECT 1 
                                                FROM "Comments" c 
                                                WHERE c."FormId" = f."Id" 
                                                AND c."CommentSearchVector" @@ plainto_tsquery('english', {searchQuery})
                                            )
                                    )
                                    SELECT "Id"
                                    FROM search_results
                                    ORDER BY (form_rank + comment_rank) DESC
                                """).ToListAsync();

            query = query.Where(f => formIds.Contains(f.Id));
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
                Description = f.Description,
                IsPublic = f.IsPublic || CheckUserinList(context, GetUserId(),f.Id),
                LikeCount = context.Likes.Count(l => l.FormId == f.Id)
            })
            .ToListAsync();

        
        var allTagStrings = await context.Forms
            .Where(f => !string.IsNullOrEmpty(f.Tags))
            .Select(f => f.Tags)
            .ToListAsync();

        var allTags = allTagStrings
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
            SelectedTag = tag,
            SearchQuery = searchQuery,
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