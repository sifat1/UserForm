namespace UserForm.ViewModels.Admin;

public class AdminUserViewModel
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsLockedOut { get; set; }
}