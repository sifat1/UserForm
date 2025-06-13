using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Forms;

namespace UserForm.Controllers;


public class UserDatatoFormsController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ShowForm(int formId)
    {
        var form = await context.Forms.FirstOrDefaultAsync(f => f.Id == formId);
        return form == null ? NotFound(new { message = "Form not found" }):Ok(form);
    }
    
    [HttpPost]
    public async Task<IActionResult> SubmitForm(UserForms formdata)
    {
        try
        {
            context.UserForms.Add(formdata);
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return BadRequest();
        }
        
        return Ok(new { message = "Form Submitted" });
    }
}