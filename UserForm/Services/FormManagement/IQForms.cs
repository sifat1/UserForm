using Microsoft.AspNetCore.Mvc;
using UserForm.Models.DBModels.Forms;

namespace UserForm.Services.FormManagement;

public interface IQForms
{
    public IActionResult Create();
    public IActionResult AddQuestion([FromForm] Object question, [FromForm] UserForms Froms);
    public IActionResult Edit();
    public IActionResult Delete();
    public IActionResult Detail();
    public IActionResult SetTime();
}