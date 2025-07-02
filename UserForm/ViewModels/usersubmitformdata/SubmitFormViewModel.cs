namespace UserForm.ViewModels.usersubmitformdata;

public class SubmitFormViewModel
{
    public int FormId { get; set; }
    public string? FormTitle { get; set; }
    public string? FormTopic { get; set; }
    public List<QuestionViewModel> Questions { get; set; } = new();
    public List<AnswerInputModel> Answers { get; set; } = new(); 
    
    public List<CommentDisplayViewModel> Comments { get; set; } = new();
}
