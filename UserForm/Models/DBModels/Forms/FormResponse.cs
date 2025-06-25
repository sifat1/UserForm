using System.ComponentModel.DataAnnotations.Schema;
using UserForm.Models.DBModels.Question;
using UserForm.Models.DBModels.Users;

namespace UserForm.Models.DBModels.Forms;

public class FormResponse
{
    public int Id { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public int FormId { get; set; }
    public FormEntity Form { get; set; }
    
    [ForeignKey(nameof(SubmittedBy))]
    public string SubmittedById { get; set; }
    public UserDetails SubmittedBy { get; set; }
    public ICollection<AnswerEntity> Answers { get; set; } = new List<AnswerEntity>();
}
