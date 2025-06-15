using System.ComponentModel.DataAnnotations;

namespace UserForm.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(30, MinimumLength = 1)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}