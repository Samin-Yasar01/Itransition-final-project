using FormsApp.Data;
using FormsApp.Features.Templates.Models;
using FormsApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FormsApp.Features.Search.Services
{
    public interface ISearchService
    {
        Task<IEnumerable<Template>> SearchTemplatesAsync(string query, string userId);
    }

    public class SearchService : ISearchService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SearchService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private const int MinSearchTermLength = 2;
        private const int MinRelevanceScore = 1;

        public SearchService(ApplicationDbContext context, ILogger<SearchService> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Template>> SearchTemplatesAsync(string query, string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Enumerable.Empty<Template>();
                }

                var searchTerms = ProcessSearchTerms(query);
                if (!searchTerms.Any())
                {
                    return Enumerable.Empty<Template>();
                }

                return await SearchTemplatesWithTermsAsync(searchTerms, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching templates");
                throw;
            }
        }

        private List<string> ProcessSearchTerms(string query)
        {
            return query.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(term => term.Trim().ToLowerInvariant())
                .Where(term => term.Length >= MinSearchTermLength)
                .ToList();
        }

        private async Task<IEnumerable<Template>> SearchTemplatesWithTermsAsync(List<string> searchTerms, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

            // Build the base query with all necessary includes
            var query = _context.Templates
                .Include(t => t.Questions)
                .Include(t => t.Comments)
                .Include(t => t.User)
                .Include(t => t.TemplateTags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.AllowedUsers)
                .Where(t => 
                    t.IsPublic || // Public templates
                    t.UserId == userId || // Templates owned by the user
                    t.AllowedUsers.Any(ta => ta.UserId == userId) || // Templates shared with the user
                    isAdmin // Admin can see all templates
                );

            // Apply search terms to the query
            foreach (var term in searchTerms)
            {
                query = query.Where(t =>
                    EF.Functions.ILike(t.Title, $"%{term}%") ||
                    EF.Functions.ILike(t.Description, $"%{term}%") ||
                    t.TemplateTags.Any(tt => EF.Functions.ILike(tt.Tag.Name, $"%{term}%")) ||
                    t.Questions.Any(q => EF.Functions.ILike(q.Description, $"%{term}%")) ||
                    t.Comments.Any(c => EF.Functions.ILike(c.Content, $"%{term}%"))
                );
            }

            // Execute the query and get results
            var templates = await query.ToListAsync();

            // Calculate relevance scores
            var scoredTemplates = templates
                .Select(t => new
                {
                    Template = t,
                    Score = CalculateRelevanceScore(t, searchTerms)
                })
                .Where(x => x.Score >= MinRelevanceScore)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Template)
                .ToList();

            return scoredTemplates;
        }

        private int CalculateRelevanceScore(Template template, List<string> searchTerms)
        {
            var score = 0;
            var titleLower = template.Title.ToLowerInvariant();
            var descriptionLower = template.Description.ToLowerInvariant();

            foreach (var term in searchTerms)
            {
                // Exact title match is most important
                if (titleLower.Equals(term, StringComparison.OrdinalIgnoreCase))
                {
                    score += 5;
                }
                // Title contains the term
                else if (titleLower.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    score += 3;
                }

                // Description contains the term
                if (descriptionLower.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    score += 2;
                }

                // Question matches
                score += template.Questions
                    .Count(q => q.Description.ToLowerInvariant().Contains(term, StringComparison.OrdinalIgnoreCase));

                // Comment matches
                score += template.Comments
                    .Count(c => c.Content.ToLowerInvariant().Contains(term, StringComparison.OrdinalIgnoreCase));

                // Tag matches
                score += template.TemplateTags
                    .Count(tt => tt.Tag.Name.ToLowerInvariant().Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            return score;
        }
    }
}