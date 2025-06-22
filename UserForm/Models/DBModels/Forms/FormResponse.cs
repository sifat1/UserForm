using UserForm.Models.DBModels.Question;

namespace UserForm.Models.DBModels.Forms;

public class FormResponse
{
    public int Id { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Foreign key to form
    public int FormId { get; set; }
    public FormEntity Form { get; set; }

    // Navigation property: one response has many answers
    public ICollection<AnswerEntity> Answers { get; set; } = new List<AnswerEntity>();
}
