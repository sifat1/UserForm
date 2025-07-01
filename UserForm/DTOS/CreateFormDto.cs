using Microsoft.AspNetCore.Mvc.Rendering;

namespace  UserForm.DTOS;
public class CreateFormDto
{
    public int? Id { get; set; }
    public string FormTitle { get; set; }
    public string? FormDescription { get; set; }
    public string FormTopic { get; set; }
    public string Tags { get; set; } 
    public bool IsPublic { get; set; }
    public List<QuestionDto> Questions { get; set; }
    public List<string> Topics { get; set; } = new List<string>();
}

