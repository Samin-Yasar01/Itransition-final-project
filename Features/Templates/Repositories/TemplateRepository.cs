using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FormsApp.Features.Templates.Models;
using FormsApp.Data;

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
                var entry = _context.Entry(template);
                entry.State = EntityState.Modified;

                // Handle concurrency using xmin shadow property
                if (entry.Property("xmin").CurrentValue == null)
                {
                    await _context.Entry(template).GetDatabaseValuesAsync();
                }

                await _context.SaveChangesAsync();
                return template;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var databaseValues = await entry.GetDatabaseValuesAsync();

                if (databaseValues == null)
                {
                    return null;
                }

                entry.OriginalValues.SetValues(databaseValues);
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