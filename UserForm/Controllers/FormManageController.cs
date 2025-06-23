using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels;
using UserForm.ViewModels.FormManage;

namespace UserForm.Controllers;

public class FormManageController(AppDbContext context) : Controller
{
    AppDbContext _context = context;

    [HttpGet]
    public async Task<IActionResult> List(string? topic, string? tag, int page = 1, int pageSize = 6)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = _context.Forms.AsQueryable();

        // Filter by topic
        if (!string.IsNullOrEmpty(topic))
            query = query.Where(f => f.FormTopic == topic);

        // Filter by tag (exact match within CSV-style string)
        if (!string.IsNullOrEmpty(tag))
            query = query.Where(f => f.Tags.Contains(tag)); // Optional: improve for exact match

        // Pagination
        var totalCount = await query.CountAsync();
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
                IsPublic = f.IsPublic
            }).ToListAsync();

        // Fetch topics and tags AFTER executing the filtered query
        var allTopics = await _context.Forms
            .Select(f => f.FormTopic)
            .Distinct()
            .ToListAsync();

        // Tags must be handled in-memory
        var allTagStrings = await _context.Forms
            .Select(f => f.Tags)
            .ToListAsync();

        var allTags = allTagStrings
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .SelectMany(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(tag => tag.Trim())
            .Distinct()
            .OrderBy(tag => tag)
            .ToList();

        var vm = new PaginatedFormListViewModel
        {
            Forms = forms,
            CurrentPage = page,
            TotalPages = totalPages,
            CurrentUserId = currentUserId,
            SelectedTopic = topic,
            SelectedTag = tag,
            AvailableTopics = allTopics,
            AvailableTags = allTags
        };

        return View(vm);
    }
}