using System.ComponentModel.DataAnnotations;
using FormGenerator.Models.DBModels.Question;

namespace UserForm.Models.DBModels;

public class BaseForm
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; }

    // Navigation: One form template has many base questions
    public ICollection<BaseQuestion> Questions { get; set; } = new List<BaseQuestion>();
}