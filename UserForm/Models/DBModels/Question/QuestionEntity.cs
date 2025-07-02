using System.ComponentModel.DataAnnotations;
using UserForm.Models.DBModels.Forms;

namespace UserForm.Models.DBModels.Question;

public class QuestionEntity
{
    public int Id { get; set; }

    [Required]
    public string QuestionText { get; set; }

    [Required]
    public string QuestionType { get; set; } 
    
    public int FormId { get; set; }
    public FormEntity Form { get; set; }
    
    public ICollection<OptionEntity> Options { get; set; } = new List<OptionEntity>();
    public ICollection<AnswerEntity> Answers { get; set; } = new List<AnswerEntity>();
}