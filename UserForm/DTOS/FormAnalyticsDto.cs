namespace UserForm.DTOS;

public class FormAnalyticsDto
{
    public int FormId { get; set; }
    public string FormTitle { get; set; }
    public List<QuestionAnalyticsDto> Questions { get; set; } = new();
}