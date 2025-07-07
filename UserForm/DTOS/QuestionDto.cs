using UserForm.Models.DBModels.Question;

namespace UserForm.DTOS;


public class QuestionDto
{
    public string QuestionText { get; set; }
    public string QuestionType { get; set; } 
    public List<string>? Options { get; set; }
}