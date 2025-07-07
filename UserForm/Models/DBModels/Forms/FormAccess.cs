using UserForm.Models.DBModels.Users;

namespace UserForm.Models.DBModels.Forms;

public class FormAccess
{
    public int Id { get; set; }

    public int FormId { get; set; }
    public FormEntity Form { get; set; }

    public string UserId { get; set; }
    public UserDetails User { get; set; }
}
