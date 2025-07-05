using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserForm.Models.DBModels.Forms;
using UserForm.Models.DBModels.Question;
using UserForm.Models.DBModels.Users;

namespace UserForm.Models.DBModels;

public class AppDbContext : IdentityDbContext<UserDetails, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<TopicEntity> Topics { get; set; }
    public DbSet<FormEntity> Forms { get; set; }
    public DbSet<LikeEntity> Likes { get; set; }
    public DbSet<CommentEntity> Comments { get; set; }
    public DbSet<QuestionEntity> Questions { get; set; }
    public DbSet<OptionEntity> Options { get; set; }
    public DbSet<FormResponse> FormResponses { get; set; }
    public DbSet<AnswerEntity> Answers { get; set; } 
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserDetails>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        modelBuilder.Entity<FormEntity>()
            .HasGeneratedTsVectorColumn(
                f => f.FormSearchVector,
                "english", 
                f => new { f.Description } 
            )
            .HasIndex(f => f.FormSearchVector)
            .HasMethod("GIN");

        
        modelBuilder.Entity<CommentEntity>()
            .HasGeneratedTsVectorColumn(
                c => c.CommentSearchVector,
                "english", 
                c => new { c.Content }
            )
            .HasIndex(c => c.CommentSearchVector)
            .HasMethod("GIN");
        
        modelBuilder.Entity<FormEntity>()
            .HasMany(f => f.Questions)
            .WithOne(q => q.Form)
            .HasForeignKey(q => q.FormId) 
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<FormEntity>()
            .HasIndex(f => f.FormTopic);
        
        modelBuilder.Entity<QuestionEntity>()
            .HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId) 
            .OnDelete(DeleteBehavior.Cascade);

        
        modelBuilder.Entity<FormEntity>()
            .HasMany(f => f.Responses)
            .WithOne(r => r.Form)
            .HasForeignKey(r => r.FormId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<FormResponse>()
            .HasMany(r => r.Answers)
            .WithOne(a => a.FormResponse)
            .HasForeignKey(a => a.FormResponseId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<QuestionEntity>()
            .HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<LikeEntity>()
            .HasIndex(l => new { l.UserId, l.FormId })
            .IsUnique(); 
    }
}
