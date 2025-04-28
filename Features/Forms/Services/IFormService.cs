using FormsApp.Features.Forms.Models;

namespace FormsApp.Features.Forms.Services
{
    public interface IFormService
    {
        Task<Form> CreateForm(Guid templateId, string userId);
        Task<Form> GetFormWithAnswers(Guid formId);
        Task<Form> GetFormWithAnswersAndSnapshots(Guid formId);
        Task SubmitForm(Form form);
        Task<IEnumerable<Form>> GetUserForms(string userId);
        Task<IEnumerable<Form>> GetFormsByTemplate(Guid templateId);
        Task<IEnumerable<Form>> GetAllFormsAsync();
        Task UpdateFormAsync(Form form);
        Task DeleteFormAsync(Guid id);
    }
}