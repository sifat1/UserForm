using System.ComponentModel.DataAnnotations;
using UserForm.Models.DBModels.Users;

namespace UserForm.Models.DBModels.Forms;

public class CommentEntity
{
    public int Id { get; set; }

    public string UserId { get; set; }
    public UserDetails User { get; set; }

    public int FormId { get; set; }
    public FormEntity Form { get; set; }

    [Required]
    public string Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
