using FormGenerator.Models.DBModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserForm.DTOS;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Forms;
using UserForm.Models.DBModels.Users;

namespace UserForm.Controllers;

public class FormsController(AppDbContext context,UserManager<UserDetails> userManager) : Controller
{
    [HttpGet]
    public ViewResult CreateForm()
    {
        return View();
    }

    [HttpPost]
    public ViewResult CreateForm(CreateFormDto model)
    {
        if (!ModelState.IsValid)
            return View();

        var form = new BaseForm { Title = model.Title, Questions = model.Questions };
        var Form = context.Add(form);
        var UserForms = new UserForms
            {
                FormownerId = int.Parse(userManager.GetUserId(User) ?? string.Empty),
                FormTemplateId = form.Id,
            };
        context.SaveChangesAsync();

        return View();
    }

    public async Task<ActionResult> UpdateForm(CreateFormDto model)
    {
        if (!ModelState.IsValid) return Redirect("/");
        var form = new BaseForm { Title = model.Title, Questions = model.Questions };
        context.Update(form);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<ActionResult> DeleteForm(CreateFormDto model)
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