
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels.Forms;
using UserForm.Models.DBModels.Users;

namespace UserForm.Models.DBModels;

public class AppDbContext : IdentityDbContext<UserDetails>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<FormEntity> Forms { get; set; }
    public DbSet<QuestionEntity> Questions { get; set; }
    public DbSet<OptionEntity> Options { get; set; }
    public DbSet<FormResponse> FormResponses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FormEntity>()
            .HasMany(f => f.Questions)
            .WithOne(q => q.Form)
            .HasForeignKey(q => q.FormEntityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuestionEntity>()
            .HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionEntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}