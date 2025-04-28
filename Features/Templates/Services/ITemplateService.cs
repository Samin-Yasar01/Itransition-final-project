using FormsApp.Features.Templates.Models;

namespace FormsApp.Features.Templates.Services
{
    public interface ITemplateService
    {
        Task<Template> CreateTemplateAsync(Template template);
        Task<Template> GetTemplateByIdAsync(Guid id);
        Task<IEnumerable<Template>> GetTemplatesByUserIdAsync(string userId);
        Task<IEnumerable<Template>> GetAllTemplatesAsync();
        Task<Template> UpdateTemplateAsync(Template template);
        Task DeleteTemplateAsync(Guid id);
        Task<TemplateDetailsModel?> GetTemplateDetailsAsync(Guid templateId, string userId);
        Task ReorderQuestions(Guid templateId, List<Guid> questionOrder);
        Task<bool> HasAccessToTemplate(Guid templateId, string userId);
        Task<bool> TemplateExistsAsync(Guid id);
    }
}