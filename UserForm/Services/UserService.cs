using UserForm.Models.DBModels;

public class UserService
{
    private AppDbContext _db;
    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public bool IfUserExists(string Email)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == Email && u.IsBlocked != true);

        return user != null ? true : false;
    }
}