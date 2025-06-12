using FormGenerator.Models.DBModels.Question;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels;
using UserForm.Models.DBModels.Forms;

namespace FormGenerator.Services.FormManagement;

public class ManageForms(DbContext db) : IQForms
{
    private readonly DbContext _db = db;

    public IActionResult Create()
    {
        throw new NotImplementedException();
    }

    public IActionResult AddQuestion(object question, UserForms Froms)
    {
        throw new NotImplementedException();
    }

    public IActionResult AddQuestion([FromForm] BaseQuestion question, [FromForm] UserForms Froms)
    {
        Froms.FormTemplate.Questions.Add(question);
        _db.UpdateRange(Froms.FormTemplate.Questions);
        _db.SaveChanges();
        
        throw new NotImplementedException();
    }

    public IActionResult Edit()
    {
        throw new NotImplementedException();
    }

    public IActionResult Delete()
    {
        throw new NotImplementedException();
    }

    public IActionResult Detail()
    {
        throw new NotImplementedException();
    }

    public IActionResult SetTime()
    {
        throw new NotImplementedException();
    }
}