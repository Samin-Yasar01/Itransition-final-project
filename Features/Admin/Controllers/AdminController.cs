using FormsApp.Features.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FormsApp.Models;
using FormsApp.Features.Admin.Models;
using System.Security.Claims;

namespace FormsApp.Features.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISalesforceService _salesforceService;

        public AdminController(IAdminService adminService, UserManager<ApplicationUser> userManager, ISalesforceService salesforceService)
        {
            _adminService = adminService;
            _userManager = userManager;
            _salesforceService = salesforceService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _adminService.GetAllUsersAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUser(string userId, string action)
        {
            bool success = action switch
            {
                "toggle-admin" => await _adminService.ToggleAdminAsync(userId),
                "toggle-block" => await _adminService.ToggleBlockAsync(userId),
                "delete" => await _adminService.DeleteUserAsync(userId),
                _ => false
            };

            return success ? RedirectToAction("Index") : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAdminRole(string userId)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot remove admin role from yourself.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return RedirectToAction(nameof(Index));
        }
        [Authorize]
        public async Task<IActionResult> SalesforceIntegration()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(new SalesforceIntegrationModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalesforceIntegration(SalesforceIntegrationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var success = await _salesforceService.CreateSalesforceAccount(user, model);

            if (success)
            {
                TempData["SuccessMessage"] = "Your information has been successfully sent to Salesforce!";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "There was an error connecting to Salesforce. Please try again.");
            return View(model);
        }
    }
}