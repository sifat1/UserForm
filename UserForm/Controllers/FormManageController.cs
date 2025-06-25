using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Forms;
using UserForm.ViewModels.Analytics;
using UserForm.ViewModels.FormManage;

namespace UserForm.Controllers;

public class FormManageController(AppDbContext context) : Controller
{
    AppDbContext _context = context;

    [HttpGet("/")]
    public async Task<IActionResult> List(string? topic, string? tag, string? searchQuery, int page = 1, int pageSize = 6)
    {
        var topTemplates = await GetTopTemplatesAsync();
        ViewData["TopTemplates"] = topTemplates;

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = _context.Forms.AsQueryable();
        if (!string.IsNullOrEmpty(topic))
            query = query.Where(f => f.FormTopic == topic);

        if (!string.IsNullOrEmpty(tag))
            query = query.Where(f => f.Tags.Contains(tag));

        if (!string.IsNullOrEmpty(searchQuery))
            query = query.Where(f => f.FormTitle.Contains(searchQuery));

        // Pagination
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
                LikeCount = _context.Likes.Count(l => l.FormId == f.Id)
            }).ToListAsync();

        // Topics and Tags
        var allTopics = await _context.Forms
            .Select(f => f.FormTopic)
            .Distinct()
            .ToListAsync();

        var allTagStrings = await _context.Forms
            .Select(f => f.Tags)
            .ToListAsync();

        var allTags = allTagStrings
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .SelectMany(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(t => t.Trim())
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        // ViewModel
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

        var alreadyLiked = await _context.Likes.AnyAsync(l => l.FormId == formId && l.UserId == userId);

        if (!alreadyLiked)
        {
            _context.Likes.Add(new LikeEntity { FormId = formId, UserId = userId });
            await _context.SaveChangesAsync();
        }

        return !string.IsNullOrEmpty(returnUrl) ? Redirect(returnUrl) : RedirectToAction("List");
    }
    
    
    [HttpPost]
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
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("List"); // or form details page
    }
    
    public async Task<List<PopularTemplateViewModel>> GetTopTemplatesAsync()
    {
        return await _context.FormResponses
            .GroupBy(r => r.FormId)
            .Select(g => new
            {
                FormId = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(g => g.Count)
            .Take(5)
            .Join(_context.Forms,
                r => r.FormId,
                f => f.Id,
                (r, f) => new PopularTemplateViewModel
                {
                    FormId = f.Id,
                    FormTitle = f.FormTitle,
                    OwnerEmail = f.CreatedBy.Email,
                    SubmissionCount = r.Count
                })
            .ToListAsync();
    }


}