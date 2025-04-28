using FormsApp.Features.Templates.Models;

namespace FormsApp.Features.Templates.Repositories
{
    public interface ITemplateRepository
    {
        Task<Template> CreateTemplateAsync(Template template);
        Task<Template> GetTemplateByIdAsync(Guid id);
        Task<IEnumerable<Template>> GetTemplatesByUserIdAsync(string userId);
        Task<Template> UpdateTemplateAsync(Template template);
        Task DeleteTemplateAsync(Guid id);
    }
} 