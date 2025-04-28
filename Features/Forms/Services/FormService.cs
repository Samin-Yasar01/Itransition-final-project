using FormsApp.Data;
using FormsApp.Features.Forms.Models;
using FormsApp.Features.Templates.Services;
using Microsoft.EntityFrameworkCore;

namespace FormsApp.Features.Forms.Services
{
    public class FormService : IFormService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITemplateService _templateService;

        public FormService(ApplicationDbContext context, ITemplateService templateService)
        {
            _context = context;
            _templateService = templateService;
        }

        public async Task<Form> CreateForm(Guid templateId, string userId)
        {
            var template = await _templateService.GetTemplateByIdAsync(templateId);
            if (template == null) throw new Exception("Template not found");

            var form = new Form
            {
                TemplateId = templateId,
                UserId = userId,
                // Ensure snapshots are created from the template's questions
                QuestionSnapshots = template.Questions.Select(q => new QuestionSnapshot
                {
                    OriginalQuestionId = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    Type = q.Type,
                    AllowEmpty = q.AllowEmpty,
                    Order = q.Order
                }).ToList()
            };

            _context.Forms.Add(form);
            await _context.SaveChangesAsync();
            return form;
        }

        public async Task<Form> GetFormWithAnswers(Guid formId)
        {
            return await _context.Forms
                .Include(f => f.Answers)
                .Include(f => f.Template)
                    .ThenInclude(t => t.Questions)
                .Include(f => f.Template)
                    .ThenInclude(t => t.User)
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == formId);
        }

        public async Task<Form> GetFormWithAnswersAndSnapshots(Guid formId)
        {
            return await _context.Forms
                .Include(f => f.Template)
                    .ThenInclude(t => t.User)
                .Include(f => f.User)
                .Include(f => f.QuestionSnapshots)
                .Include(f => f.Answers)
                .FirstOrDefaultAsync(f => f.Id == formId);
        }

        public async Task SubmitForm(Form form)
        {
            form.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Form>> GetUserForms(string userId)
        {
            return await _context.Forms
                .Where(f => f.UserId == userId)
                .Include(f => f.Template)
                    .ThenInclude(t=>t.User)
                .OrderByDescending(f => f.UpdatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Form>> GetFormsByTemplate(Guid templateId)
        {
            return await _context.Forms
                .Where(f => f.TemplateId == templateId)
                .Include(f => f.User)
                .Include(f => f.Answers)
                .ToListAsync();
        }

        public async Task<IEnumerable<Form>> GetAllFormsAsync()
        {
            return await _context.Forms
                .Include(f => f.Answers)
                .Include(f => f.QuestionSnapshots)
                .Include(f => f.Template)
                .Include(f => f.User)
                .ToListAsync();
        }

        public async Task UpdateFormAsync(Form form)
        {
            _context.Forms.Update(form);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFormAsync(Guid id)
        {
            var form = await _context.Forms.FindAsync(id);
            if (form != null)
            {
                _context.Forms.Remove(form);
                await _context.SaveChangesAsync();
            }
        }
    }
}