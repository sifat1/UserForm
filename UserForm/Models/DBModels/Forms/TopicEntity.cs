using System.ComponentModel.DataAnnotations;

namespace UserForm.Models.DBModels.Forms;

public class TopicEntity
{
    [Key]
    public int Id { get; set; }
    public string TopicName { get; set; } = String.Empty;
}