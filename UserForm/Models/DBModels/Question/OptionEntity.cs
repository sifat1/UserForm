using System.ComponentModel.DataAnnotations;

namespace UserForm.Models.DBModels.Question;

public class OptionEntity
{
    public int Id { get; set; }

    [Required]
    public string OptionText { get; set; }
    public int QuestionId { get; set; }
    public QuestionEntity Question { get; set; }
}