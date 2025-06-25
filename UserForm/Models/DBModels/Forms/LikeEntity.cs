using UserForm.Models.DBModels.Users;

namespace UserForm.Models.DBModels.Forms;

public class LikeEntity
{
    public int Id { get; set; }

    public string UserId { get; set; }
    public UserDetails User { get; set; }

    public int FormId { get; set; }
    public FormEntity Form { get; set; }

    public DateTime LikedAt { get; set; } = DateTime.UtcNow;
}
