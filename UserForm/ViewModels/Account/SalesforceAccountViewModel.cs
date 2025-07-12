using System.ComponentModel.DataAnnotations;

namespace UserForm.ViewModels.Account;
public class SalesforceAccountViewModel
{
    public string UserId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Company { get; set; }

    [Phone]
    public string Phone { get; set; }
}
