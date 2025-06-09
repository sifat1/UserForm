using Microsoft.AspNetCore.Identity;

namespace UserForm.Models.DBModels.Users;

public class UserDetails : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime LastLogin { get; set; } = DateTime.Now;
    public bool IsBlocked { get; set; } = false;
}