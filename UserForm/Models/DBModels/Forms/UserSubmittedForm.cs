using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormGenerator.Models.DBModels.Question;
using UserForm.Models.DBModels.Users;

namespace UserForm.Models.DBModels.Forms;

public class UserSubmittedForm
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Submitter))]
    public int SubmitterId { get; set; }
    public UserDetails Submitter { get; set; }
    
    [ForeignKey(nameof(Submittedoption))]
    public int? SubmittedoptionId { get; set; }
    public Options? Submittedoption { get; set; }
    
    public string? Submittedtext { get; set; }

    [ForeignKey(nameof(BaseQuestion))]
    public int BaseQuestionId { get; set; }
    public BaseQuestion BaseQuestion { get; set; }

    [ForeignKey(nameof(UserForms))]
    public int UserFormsId { get; set; }
    public UserForms UserForms { get; set; }
}