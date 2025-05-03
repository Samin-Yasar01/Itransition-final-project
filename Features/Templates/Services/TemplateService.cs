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

        // Constructor: Initializes required services and dependencies
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

        // Creates a new template and grants admin users full access
        public async Task<Template> CreateTemplateAsync(Template template)
        {
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

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

        // Gets a template by ID with all related entities
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

        // Gets all templates for a specific user
        public async Task<IEnumerable<Template>> GetTemplatesByUserIdAsync(string userId)
        {
            return await _templateRepository.GetTemplatesByUserIdAsync(userId);
        }

        // Gets all templates with complete related data
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

        // Updates an existing template
        public async Task<Template> UpdateTemplateAsync(Template template)
        {
            return await _templateRepository.UpdateTemplateAsync(template);
        }

        // Deletes a template and all its related data
        public async Task DeleteTemplateAsync(Guid id)
        {
            await _templateRepository.DeleteTemplateAsync(id);
        }

        // Gets template details including social engagement metrics
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

        // Reorders questions in a template based on provided order
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

        // Checks if a user has access to a specific template
        public async Task<bool> HasAccessToTemplate(Guid templateId, string userId)
        {
            var template = await GetTemplateByIdAsync(templateId);
            if (template == null) return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Admins always have access
            if (await _userManager.IsInRoleAsync(user, "Admin")) return true;

            // Public templates are accessible to everyone
            if (template.IsPublic) return true;

            // Check owner or explicit access
            return template.UserId == userId ||
                   template.AllowedUsers.Any(ta => ta.UserId == userId);
        }

        // Checks if a template exists
        public async Task<bool> TemplateExistsAsync(Guid id)
        {
            return await _context.Templates.AnyAsync(t => t.Id == id);
        }
    }

    // Model for returning template details with social metrics
    public class TemplateDetailsModel
    {
        public required Template Template { get; set; }
        public int LikeCount { get; set; }
        public bool HasLiked { get; set; }
    }
}