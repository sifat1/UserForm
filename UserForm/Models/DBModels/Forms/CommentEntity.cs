using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NpgsqlTypes;
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
    
    [Column(TypeName = "tsvector")]
    public NpgsqlTsVector CommentSearchVector { get; set; }
}
