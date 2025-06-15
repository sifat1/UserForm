namespace UserForm.Models.ViewModels;

public class QuestionInputViewModel
{
    public string QuestionType { get; set; } = string.Empty; // e.g., "Text" or "Options"
    public string QuestionText { get; set; } = string.Empty;

    // Only used for "Options" questions
    public List<string>? Options { get; set; }
    public int? CorrectOptionIndex { get; set; }
}
