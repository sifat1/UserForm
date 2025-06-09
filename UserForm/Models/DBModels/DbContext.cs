using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels.Forms;
using UserForm.Models.DBModels.Users;

namespace UserForm.Models.DBModels;

public class DbContext : IdentityDbContext<UserDetails>
{
    public DbContext(DbContextOptions<DbContext> options) : base(options) { }

    public DbSet<UserForms> UserForms { get; set; }
    public DbSet<BaseForm> Forms { get; set; }
    public DbSet<BaseQuestion> Questions { get; set; }
    public DbSet<QuestionwithOptions> OptionQuestions { get; set; }
    public DbSet<QuestionwithTextOption> TextQuestions { get; set; }
    public DbSet<UserDetails> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserDetails>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Removed .IsUnique() so multiple forms per user are allowed
        builder.Entity<UserForms>()
            .HasIndex(u => u.UserId);

        // Set up inheritance for questions
        builder.Entity<BaseQuestion>()
            .HasDiscriminator<string>("QuestionType")
            .HasValue<QuestionwithOptions>("Options")
            .HasValue<QuestionwithTextOption>("Text");
    }
}
