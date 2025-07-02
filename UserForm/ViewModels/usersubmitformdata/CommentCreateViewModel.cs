using System.ComponentModel.DataAnnotations;

namespace UserForm.ViewModels.usersubmitformdata;

public class CommentCreateViewModel
{
    public int FormId { get; set; }

    [Required]
    public string Content { get; set; }
}
