namespace UserForm.Models.ViewModels;

public class QuestionViewModel
{
    public string QuestionText { get; set; }
    public string QuestionType { get; set; } // "Text" or "Options"

    public List<string> Options { get; set; } = new List<string>();

    public int? CorrectOptionIndex { get; set; } // Nullable to support text questions
}



