namespace UserForm.ViewModels.Admin;

public class AdminUserManagementViewModel
{
    public List<AdminUserViewModel> Users { get; set; } = new();
    public List<string> SelectedUserIds { get; set; } = new();
}