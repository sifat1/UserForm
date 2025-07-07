namespace UserForm.ViewModels.Mydashboard;

public class FormAccessManageViewModel
{
    public int FormId { get; set; }
    public List<string> AccessUserEmails { get; set; } = new();
}
