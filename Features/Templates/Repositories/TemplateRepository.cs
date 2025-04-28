using FormsApp.Data;
using FormsApp.Features.Templates.Models;
using Microsoft.EntityFrameworkCore;

namespace FormsApp.Features.Templates.Repositories
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly ApplicationDbContext _context;

        public TemplateRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Template> CreateTemplateAsync(Template template)
        {
            _context.Templates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<Template> GetTemplateByIdAsync(Guid id)
        {
            return await _context.Templates
                .Include(t => t.Questions)
                .Include(t => t.AllowedUsers)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Template>> GetTemplatesByUserIdAsync(string userId)
        {
            return await _context.Templates
                .Include(t => t.Questions)
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task<Template> UpdateTemplateAsync(Template template)
        {
            try
            {
                _context.Entry(template).State = EntityState.Modified;
                _context.Entry(template).Property(t => t.xmin).OriginalValue = template.xmin;
                await _context.SaveChangesAsync();
                return template;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var databaseValues = await _context.Entry(template).GetDatabaseValuesAsync();
                if (databaseValues == null)
                {
                    return null;
                }

                template.xmin = ((Template)databaseValues.ToObject()).xmin;
                throw;
            }
        }

        public async Task DeleteTemplateAsync(Guid id)
        {
            var template = await _context.Templates
                .Include(t => t.Questions)
                .Include(t => t.Forms)
                    .ThenInclude(f => f.Answers)
                .Include(t => t.Forms)
                    .ThenInclude(f => f.QuestionSnapshots)
                .Include(t => t.Comments)
                .Include(t => t.Likes)
                .Include(t => t.AllowedUsers)
                .Include(t => t.TemplateTags)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template != null)
            {
                // Delete all related entities
                _context.FormAnswers.RemoveRange(template.Forms.SelectMany(f => f.Answers));
                _context.QuestionSnapshots.RemoveRange(template.Forms.SelectMany(f => f.QuestionSnapshots));
                _context.Forms.RemoveRange(template.Forms);
                _context.Comments.RemoveRange(template.Comments);
                _context.Likes.RemoveRange(template.Likes);
                _context.TemplateAccesses.RemoveRange(template.AllowedUsers);
                _context.TemplateTags.RemoveRange(template.TemplateTags);
                _context.Questions.RemoveRange(template.Questions);
                _context.Templates.Remove(template);

                await _context.SaveChangesAsync();
            }
        }
    }
}