namespace UserForm.Models.ViewModels;

public class CreateFormViewModel
{
    public string FormTitle { get; set; } = string.Empty;

    public List<QuestionInputViewModel> Questions { get; set; } = new();
}