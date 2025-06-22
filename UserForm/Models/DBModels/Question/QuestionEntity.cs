using System.ComponentModel.DataAnnotations;
using UserForm.Models.DBModels.Forms;
using UserForm.Models.DBModels.Question;

public class QuestionEntity
{
    public int Id { get; set; }

    [Required]
    public string QuestionText { get; set; }

    [Required]
    public string QuestionType { get; set; } // "Text", "Number", "MultipleChoice"

    // Foreign key to parent form
    public int FormId { get; set; }
    public FormEntity Form { get; set; }

    // Navigation property: question has multiple options (only for MultipleChoice)
    public ICollection<OptionEntity> Options { get; set; } = new List<OptionEntity>();

    // Navigation property: question has many answers
    public ICollection<AnswerEntity> Answers { get; set; } = new List<AnswerEntity>();
}