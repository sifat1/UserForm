using FormGenerator.Models.DBModels.Question;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels;
using UserForm.Models.ViewModels;

namespace UserForm.Controllers;

public class UserDatatoFormsController(AppDbContext context, ILogger<UserDatatoFormsController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> ShowForm(int formId)
    {
        var form = await context.Forms.FirstOrDefaultAsync(f => f.Id == formId);

        if (form == null)
        {
            logger.LogWarning("Form with ID {FormId} not found.", formId);
            return NotFound(new { message = "Form not found" });
        }

        logger.LogInformation("Retrieved form with ID {FormId}", formId);
        return Ok(form);
    }
}
