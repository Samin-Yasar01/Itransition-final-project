using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FormsApp.Models;
using FormsApp.Features.Templates.Models;
using FormsApp.Features.Forms.Models;
using FormsApp.Features.Social.Models;
using FormsApp.Features.Admin.Models;

namespace FormsApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Template> Templates { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Form> Forms { get; set; }
        public DbSet<FormAnswer> FormAnswers { get; set; }
        public DbSet<QuestionSnapshot> QuestionSnapshots { get; set; }
        public DbSet<TemplateAccess> TemplateAccesses { get; set; }
        public DbSet<TemplateTag> TemplateTags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Template-User relationship
            modelBuilder.Entity<Template>(entity =>
            {
                entity.HasOne(t => t.User)
                      .WithMany(u => u.Templates)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Comment relationships
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasOne(c => c.Template)
                      .WithMany(t => t.Comments)
                      .HasForeignKey(c => c.TemplateId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.Answer)
                      .WithMany(a => a.Comments)
                      .HasForeignKey(c => c.AnswerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure other relationships
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasOne(q => q.Template)
                      .WithMany(t => t.Questions)
                      .HasForeignKey(q => q.TemplateId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Form>(entity =>
            {
                entity.HasOne(f => f.Template)
                      .WithMany(t => t.Forms)
                      .HasForeignKey(f => f.TemplateId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.User)
                      .WithMany(u => u.Forms)
                      .HasForeignKey(f => f.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<FormAnswer>(entity =>
            {
                entity.HasOne(a => a.Form)
                      .WithMany(f => f.Answers)
                      .HasForeignKey(a => a.FormId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Question)
                      .WithMany()
                      .HasForeignKey(a => a.QuestionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasOne(l => l.Template)
                      .WithMany(t => t.Likes)
                      .HasForeignKey(l => l.TemplateId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(l => l.Answer)
                      .WithMany(a => a.Likes)
                      .HasForeignKey(l => l.AnswerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.User)
                      .WithMany()
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired();
            });

            modelBuilder.Entity<TemplateAccess>(entity =>
            {
                entity.HasKey(ta => new { ta.TemplateId, ta.UserId });
                entity.HasOne(ta => ta.Template)
                      .WithMany(t => t.AllowedUsers)
                      .HasForeignKey(ta => ta.TemplateId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ta => ta.User)
                      .WithMany()
                      .HasForeignKey(ta => ta.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TemplateTag>(entity =>
            {
                entity.HasKey(tt => new { tt.TemplateId, tt.TagId });
                entity.HasOne(tt => tt.Template)
                      .WithMany(t => t.TemplateTags)
                      .HasForeignKey(tt => tt.TemplateId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(tt => tt.Tag)
                      .WithMany(t => t.TemplateTags)
                      .HasForeignKey(tt => tt.TagId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<QuestionSnapshot>(entity =>
            {
                entity.HasOne(qs => qs.Form)
                      .WithMany(f => f.QuestionSnapshots)
                      .HasForeignKey(qs => qs.FormId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}