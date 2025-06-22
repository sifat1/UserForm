namespace UserForm.Models.ViewModels;

public class AnswerInputModel
{
    public int QuestionId { get; set; }
    public string? TextAnswer { get; set; }
    public double? NumberAnswer { get; set; }
    public string? SelectedOption { get; set; }
}