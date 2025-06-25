using UserForm.Models.DBModels.Forms;

namespace UserForm.ViewModels.FormManage;

public class UserFormListViewModel
{
    public List<FormCardViewModel> Forms { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
