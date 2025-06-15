using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserForm.Models.DBModels.Forms;

namespace FormGenerator.Models.DBModels.Question;

public abstract class BaseQuestion
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Questiontxt { get; set; }

    public ICollection<UserSubmittedForm> UserSubmittedForms { get; set; }
}