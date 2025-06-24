namespace UserForm.ViewModels.FormManage;

public class PaginatedFormListViewModel
{
    public List<FormCardViewModel> Forms { get; set; } = new();

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }

    public string? CurrentUserId { get; set; }  // Made nullable for safety

    public string? SelectedTopic { get; set; }
    public string? SelectedTag { get; set; }
    public string? SearchQuery { get; set; }    // New field for search input

    public List<string> AvailableTopics { get; set; } = new();
    public List<string> AvailableTags { get; set; } = new();
}


