namespace UserForm.DTOS;

public class QuestionAnalyticsDto
{
    public string QuestionText { get; set; }
    public string QuestionType { get; set; }
    public double? AverageNumberAnswer { get; set; }  // For number questions
    public Dictionary<string, int> OptionFrequencies { get; set; } = new();  // For MCQ
}