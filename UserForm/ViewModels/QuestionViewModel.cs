namespace UserForm.Models.ViewModels;

public class QuestionViewModel
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; }
    public string QuestionType { get; set; }
    public List<string>? Options { get; set; }
}