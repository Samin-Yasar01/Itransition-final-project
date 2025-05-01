using FormsApp.Features.Search.Services;
using FormsApp.Features.Templates.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FormsApp.Models;
using System.Linq;

namespace FormsApp.Features.Search.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SearchController(ISearchService searchService, UserManager<ApplicationUser> userManager)
        {
            _searchService = searchService;
            _userManager = userManager;
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
            var userId = _userManager.GetUserId(User);
            var results = await _searchService.SearchTemplatesAsync(searchQuery, userId);
            return View(results.ToList());
        }
    }
}