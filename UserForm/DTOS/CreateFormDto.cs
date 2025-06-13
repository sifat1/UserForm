using FormGenerator.Models.DBModels.Question;

namespace  UserForm.DTOS;
public class CreateFormDto
{
    public string Title { get; set; } = string.Empty;
    public List<BaseQuestion> Questions { get; set; } = new();
}
