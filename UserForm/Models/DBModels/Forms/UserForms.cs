using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserForm.Models.DBModels.Users;

namespace UserForm.Models.DBModels.Forms;

public class UserForms
{
    [Required]
    [Key]
    public int Id { get; set; }
    [ForeignKey("userid")]
    public int UserId { get; set; }
    
    public UserDetails User { get; set; } = null!;
    [Required]
    public BaseForm FormTemplate { get; set; } = new BaseForm();
}