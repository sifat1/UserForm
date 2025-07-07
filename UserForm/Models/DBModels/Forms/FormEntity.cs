using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NpgsqlTypes;
using UserForm.Models.DBModels.Question;
using UserForm.Models.DBModels.Users;

namespace UserForm.Models.DBModels.Forms;

public class FormEntity
{
    public int Id { get; set; }

    [Required]
    public string FormTitle { get; set; }
    public string? Description {get;set;}
    public string FormTopic { get; set; }
    public string Tags { get; set; } 

    public bool IsPublic { get; set; }
    
    [ForeignKey(nameof(CreatedBy))]
    public string? CreatedById { get; set; }
    public UserDetails CreatedBy { get; set; }
    public ICollection<QuestionEntity> Questions { get; set; } = new List<QuestionEntity>();
    public ICollection<FormResponse> Responses { get; set; } = new List<FormResponse>();
    public ICollection<CommentEntity> Comments { get; set; } = new List<CommentEntity>();
    public ICollection<FormAccess> SharedWithUsers { get; set; }
    
    [Column(TypeName = "tsvector")] 
    public NpgsqlTsVector FormSearchVector { get; set; }
}