namespace UserForm.ViewModels.FormManage;

public class FormCardViewModel
{
    public int Id { get; set; }
    public string FormTitle { get; set; }
    public string FormTopic { get; set; }
    public bool IsPublic { get; set; }
    public string? OwnerUserId { get; set; }
    public int LikeCount { get; set; } = 0;
}
