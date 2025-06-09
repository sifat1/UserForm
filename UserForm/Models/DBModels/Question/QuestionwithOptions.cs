using System.ComponentModel.DataAnnotations;
using System.Linq;
namespace UserForm.Models.DBModels;

public class QuestionwithOptions : BaseQuestion
{
    public int OptionsCount { get; set; }
    [Required]
    public List<Options> List_Options { get; set; }
    public int Answer { get; set; } 
}