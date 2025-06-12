using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormGenerator.Models.DBModels.Question;

public class Options
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string OptionText { get; set; }

    public bool IsSelected { get; set; } = false;

    [ForeignKey(nameof(Question))]
    public int QuestionId { get; set; }
    public QuestionwithOptions Question { get; set; }
}