using System.ComponentModel.DataAnnotations;

namespace FormGenerator.Models.DBModels.Question;

public class QuestionwithOptions : BaseQuestion
{
    public int OptionsCount { get; set; }

    [Required]
    public ICollection<Options> Options { get; set; }

    public int? OptionAnswer { get; set; } 
}