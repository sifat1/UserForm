using System.ComponentModel.DataAnnotations;

namespace UserForm.Models.DBModels;

public class BaseForm
{    
    [Key]
    public int Id { get; set; }
    public List<Object> Questions { get; set; } = new List<object>();
    
}