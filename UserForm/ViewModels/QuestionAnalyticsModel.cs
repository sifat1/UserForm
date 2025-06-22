namespace UserForm.Models.ViewModels;

public class QuestionAnalyticsModel
{
    public string QuestionText { get; set; }
    public string QuestionType { get; set; }
    public double? Average { get; set; } // For number
    public List<OptionStat> OptionCounts { get; set; } = new(); // For MCQ
}