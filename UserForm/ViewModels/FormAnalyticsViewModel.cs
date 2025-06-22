using UserForm.Models.ViewModels;

public class FormAnalyticsViewModel
{
    public string FormTitle { get; set; }
    public string FormTopic { get; set; }
    public List<QuestionAnalyticsModel> Questions { get; set; } = new();
}