namespace UserForm.ViewModels.FormManage;

public class PaginatedFormListViewModel
{
    public List<FormCardViewModel> Forms { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string CurrentUserId { get; set; }

    public string? SelectedTopic { get; set; }
    public string? SelectedTag { get; set; }

    public List<string> AvailableTopics { get; set; } = new();
    public List<string> AvailableTags { get; set; } = new();
}

