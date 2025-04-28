using FormsApp.Data;
using FormsApp.Features.Templates.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FormsApp.Features.Search.Services
{
    public interface ISearchService
    {
        Task<IEnumerable<Template>> SearchTemplatesAsync(string query);
    }

    public class SearchService : ISearchService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SearchService> _logger;
        private const int MinSearchTermLength = 2;
        private const int MinRelevanceScore = 1;

        public SearchService(ApplicationDbContext context, ILogger<SearchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Template>> SearchTemplatesAsync(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return await GetPublicTemplatesAsync();
                }

                var searchTerms = ProcessSearchTerms(query);
                if (!searchTerms.Any())
                {
                    return await GetPublicTemplatesAsync();
                }

                return await SearchTemplatesWithTermsAsync(searchTerms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching templates");
                throw;
            }
        }

        private async Task<IEnumerable<Template>> GetPublicTemplatesAsync()
        {
            return await _context.Templates
                .Include(t => t.Questions)
                .Include(t => t.Comments)
                .Include(t => t.User)
                .Include(t => t.TemplateTags)
                    .ThenInclude(tt => tt.Tag)
                .Where(t => t.IsPublic)
                .ToListAsync();
        }

        private List<string> ProcessSearchTerms(string query)
        {
            return query.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(term => term.Trim().ToLowerInvariant())
                .Where(term => term.Length >= MinSearchTermLength)
                .ToList();
        }

        private async Task<IEnumerable<Template>> SearchTemplatesWithTermsAsync(List<string> searchTerms)
        {
            // First get all public templates with their related data
            var templates = await _context.Templates
                .Include(t => t.Questions)
                .Include(t => t.Comments)
                .Include(t => t.User)
                .Include(t => t.TemplateTags)
                    .ThenInclude(tt => tt.Tag)
                .Where(t => t.IsPublic)
                .ToListAsync();

            // Then calculate scores and filter on the client side
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
                // Title matches are most important
                if (titleLower.Contains(term))
                {
                    score += 3;
                }

                // Description matches are second most important
                if (descriptionLower.Contains(term))
                {
                    score += 2;
                }

                // Question matches
                score += template.Questions
                    .Count(q => q.Description.ToLowerInvariant().Contains(term));

                // Comment matches
                score += template.Comments
                    .Count(c => c.Content.ToLowerInvariant().Contains(term));

                // Tag matches
                score += template.TemplateTags
                    .Count(tt => tt.Tag.Name.ToLowerInvariant().Contains(term));
            }

            return score;
        }
    }
}