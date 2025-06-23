using System.ComponentModel.DataAnnotations;

namespace UserForm.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(30, MinimumLength = 4)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(30, MinimumLength = 1)]
    public string Password { get; set; } = string.Empty;
        
    [Compare("Password", ErrorMessage = "Passwords don't match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}