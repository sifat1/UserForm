using System.ComponentModel.DataAnnotations;

namespace UserForm.Models.DBModels.Forms;

public class FormEntity
{
    public int Id { get; set; }

    [Required]
    public string FormTitle { get; set; }

    public string FormTopic { get; set; }

    public string Tags { get; set; } // Comma-separated tags

    public bool IsPublic { get; set; }

    // Navigation property: form has many questions
    public ICollection<QuestionEntity> Questions { get; set; } = new List<QuestionEntity>();

    // Navigation property: form has many responses (submissions)
    public ICollection<FormResponse> Responses { get; set; } = new List<FormResponse>();
}