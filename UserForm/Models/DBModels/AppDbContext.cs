using FormGenerator.Models.DBModels.Question;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels.Forms;
using UserForm.Models.DBModels.Users;

namespace UserForm.Models.DBModels;

public class AppDbContext : IdentityDbContext<UserDetails>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<UserForms> UserForms { get; set; }
    public DbSet<BaseForm> Forms { get; set; }
    public DbSet<Options> Options { get; set; }
    public DbSet<BaseQuestion> Questions { get; set; }
    public DbSet<QuestionwithOptions> OptionQuestions { get; set; }
    public DbSet<QuestionwithTextOption> TextQuestions { get; set; }
    public DbSet<UserDetails> UserDetails { get; set; }
    public DbSet<UserSubmittedForm> UserSubmittedForms { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Unique email constraint
        builder.Entity<UserDetails>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Foreign key indices
        builder.Entity<UserForms>()
            .HasIndex(u => u.FormownerId);

        builder.Entity<UserForms>()
            .HasIndex(u => u.FormTemplateId); 
        
    }
}
