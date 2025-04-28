using FormsApp.Data;
using FormsApp.Features.Social.Services;
using FormsApp.Features.Templates.Models;
using FormsApp.Features.Templates.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FormsApp.Models;

namespace FormsApp.Features.Templates.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly ISocialService _socialService;
        private readonly ITemplateRepository _templateRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TemplateService(
            ISocialService socialService,
            ITemplateRepository templateRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _socialService = socialService;
            _templateRepository = templateRepository;
            _context = context;
            _userManager = userManager;
        }

        public async Task<Template> CreateTemplateAsync(Template template)
        {
            // Find all admin users
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            
            // Add admin access for each admin user
            foreach (var adminUser in adminUsers)
            {
                template.AllowedUsers.Add(new TemplateAccess
                {
                    UserId = adminUser.Id,
                    AccessType = "Full"
                });
            }

            return await _templateRepository.CreateTemplateAsync(template);
        }

        public async Task<Template> GetTemplateByIdAsync(Guid id)
        {
            return await _context.Templates
                .Include(t => t.Questions)
                .Include(t => t.AllowedUsers)
                .Include(t => t.User)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .Include(t => t.Likes)
                    .ThenInclude(l => l.User)
                .Include(t => t.TemplateTags)
                    .ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Template>> GetTemplatesByUserIdAsync(string userId)
        {
            return await _templateRepository.GetTemplatesByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Template>> GetAllTemplatesAsync()
        {
            return await _context.Templates
                .Include(t => t.Questions)
                .Include(t => t.AllowedUsers)
                    .ThenInclude(ta => ta.User)
                .Include(t => t.User)
                .Include(t => t.Forms)
                .Include(t => t.Comments)
                .Include(t => t.Likes)
                .Include(t => t.TemplateTags)
                    .ThenInclude(tt => tt.Tag)
                .ToListAsync();
        }

        public async Task<Template> UpdateTemplateAsync(Template template)
        {
            return await _templateRepository.UpdateTemplateAsync(template);
        }

        public async Task DeleteTemplateAsync(Guid id)
        {
            await _templateRepository.DeleteTemplateAsync(id);
        }

        public async Task<TemplateDetailsModel?> GetTemplateDetailsAsync(Guid templateId, string userId)
        {
            var template = await GetTemplateByIdAsync(templateId);
            if (template == null) return null;

            var likeCount = await _socialService.GetLikeCountAsync(templateId);
            var hasLiked = await _socialService.HasLikedAsync(templateId, userId);

            return new TemplateDetailsModel
            {
                Template = template,
                LikeCount = likeCount,
                HasLiked = hasLiked
            };
        }

        public async Task ReorderQuestions(Guid templateId, List<Guid> questionOrder)
        {
            var template = await GetTemplateByIdAsync(templateId);
            if (template == null)
            {
                throw new ArgumentException("Template not found", nameof(templateId));
            }

            foreach (var question in template.Questions)
            {
                question.Order = questionOrder.IndexOf(question.Id);
            }

            await _templateRepository.UpdateTemplateAsync(template);
        }

        public async Task<bool> HasAccessToTemplate(Guid templateId, string userId)
        {
            var template = await GetTemplateByIdAsync(templateId);
            if (template == null) return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Check if user is admin first
            if (await _userManager.IsInRoleAsync(user, "Admin")) return true;

            // If template is public, anyone can access it
            if (template.IsPublic) return true;

            // Check if user is the owner or has explicit access
            return template.UserId == userId ||
                   template.AllowedUsers.Any(ta => ta.UserId == userId);
        }

        public async Task<bool> TemplateExistsAsync(Guid id)
        {
            return await _context.Templates.AnyAsync(t => t.Id == id);
        }
    }

    public class TemplateDetailsModel
    {
        public required Template Template { get; set; }
        public int LikeCount { get; set; }
        public bool HasLiked { get; set; }
    }
}