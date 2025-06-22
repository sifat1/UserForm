using System.ComponentModel.DataAnnotations;
using UserForm.Models.DBModels.Forms;

namespace UserForm.Models.DBModels.Question;
public class AnswerEntity
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string? AnswerText { get; set; }

    public int FormResponseId { get; set; }
    public FormResponse FormResponse { get; set; }

    public QuestionEntity Question { get; set; }
}
