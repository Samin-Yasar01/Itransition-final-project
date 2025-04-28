using FormsApp.Features.Search.Services;
using FormsApp.Features.Templates.Models;
using Microsoft.AspNetCore.Mvc;

namespace FormsApp.Features.Search.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.SearchQuery = string.Empty;
            return View(new List<Template>());
        }

        [HttpPost]
        public async Task<IActionResult> Index(string searchQuery)
        {
            ViewBag.SearchQuery = searchQuery;
            var results = await _searchService.SearchTemplatesAsync(searchQuery);
            return View(results);
        }
    }
}