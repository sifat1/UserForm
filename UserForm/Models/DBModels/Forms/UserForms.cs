using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserForm.Models.DBModels.Users;

namespace UserForm.Models.DBModels.Forms;

public class UserForms
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Formowner))]
    public string FormownerId { get; set; }
    public UserDetails Formowner { get; set; }

    [Required]
    [ForeignKey(nameof(FormTemplate))]
    public int FormTemplateId { get; set; }
    public BaseForm FormTemplate { get; set; }

    // Navigation: a form instance can have many submissions
    public ICollection<UserSubmittedForm> Submissions { get; set; } = new List<UserSubmittedForm>();
}