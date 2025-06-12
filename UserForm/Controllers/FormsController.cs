using FormGenerator.DTOS;
using FormGenerator.Models.DBModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Forms;
using UserForm.Models.DBModels.Users;

namespace UserForm.Controllers;

public class FormsController(AppDbContext context,UserManager<UserDetails> userManager) : Controller
{

    [HttpPost("/Forms")]
    public async Task<ActionResult> Create(CreateFormDto model)
    {
        if (!ModelState.IsValid)
            return Redirect("/");

        var form = new BaseForm { Title = model.Title, Questions = model.Questions };
        var Form = context.Add(form);
        var UserForms = new UserForms
            {
                FormownerId = int.Parse(userManager.GetUserId(User) ?? string.Empty),
                FormTemplateId = form.Id,
            };
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<ActionResult> Update(CreateFormDto model)
    {
        if (!ModelState.IsValid) return Redirect("/");
        var form = new BaseForm { Title = model.Title, Questions = model.Questions };
        context.Update(form);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<ActionResult> Delete(CreateFormDto model)
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