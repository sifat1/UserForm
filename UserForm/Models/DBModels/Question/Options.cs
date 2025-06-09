namespace UserForm.Models.DBModels;

public class Options
{
    public int id { get; set; }
    public string option { get; set; }
    public bool is_selected { get; set; } = false;
}