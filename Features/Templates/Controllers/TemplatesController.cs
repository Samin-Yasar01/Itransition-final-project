using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FormsApp.Features.Templates.Models;
using FormsApp.Features.Templates.Services;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using FormsApp.Features.Forms.Services;
using Microsoft.AspNetCore.Identity;
using FormsApp.Models;
using FormsApp.Data;

namespace FormsApp.Features.Templates.Controllers
{
    [Authorize]
    public class TemplatesController : Controller
    {
        private readonly ITemplateService _templateService;
        private readonly IFormService _formService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        // Constructor for dependency injection
        public TemplatesController(ITemplateService templateService, IFormService fromService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _templateService = templateService;
            _formService = fromService;
            _userManager = userManager;
            _context = context;
        }

        // Displays all templates for the current user or all templates if admin
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            ViewBag.CurrentUserId = userId;
            ViewBag.IsAdmin = isAdmin;
            var templates = isAdmin
                ? await _templateService.GetAllTemplatesAsync()
                : await _templateService.GetTemplatesByUserIdAsync(userId);

            return View(templates);
        }

        // Shows template details, accessible anonymously if template is public
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id)
        {
            var template = await _templateService.GetTemplateByIdAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            if (!template.IsPublic)
            {
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", "Templates", new { id }) });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                var hasAccess = template.UserId == userId ||
                               isAdmin ||
                               template.AllowedUsers.Any(au => au.UserId == userId);

                if (!hasAccess)
                {
                    return Forbid();
                }
            }

            return View(template);
        }

        // Shows the template creation form
        public IActionResult Create()
        {
            var model = new CreateTemplateViewModel
            {
                Title = string.Empty,
                Questions = new List<QuestionViewModel>()
            };
            return View(model);
        }

        // Handles template creation form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTemplateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var template = new Template
                {
                    Title = model.Title,
                    Description = model.Description,
                    Topic = model.Topic,
                    UserId = userId,
                    IsPublic = model.Visibility == "public",
                    Questions = model.Questions.Select(q => new Question
                    {
                        Title = q.Title,
                        Description = q.Description,
                        Type = q.Type,
                        AllowEmpty = q.AllowEmpty,
                        Order = model.Questions.IndexOf(q)
                    }).ToList()
                };

                // Process tags
                if (!string.IsNullOrWhiteSpace(model.Tags))
                {
                    var tagNames = model.Tags.Split(',')
                        .Select(t => t.Trim().ToLower())
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Distinct();

                    foreach (var tagName in tagNames)
                    {
                        // Find existing tag or create new one
                        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName) 
                            ?? new Tag { Name = tagName };

                        if (tag.Id == Guid.Empty)
                        {
                            _context.Tags.Add(tag);
                            await _context.SaveChangesAsync();
                        }

                        template.TemplateTags.Add(new TemplateTag
                        {
                            TagId = tag.Id,
                            Tag = tag
                        });
                    }
                }

                if (!template.IsPublic && !string.IsNullOrWhiteSpace(model.AllowedUserEmails))
                {
                    var emails = model.AllowedUserEmails.Split(',')
                        .Select(e => e.Trim())
                        .Where(e => !string.IsNullOrWhiteSpace(e));

                    foreach (var email in emails)
                    {
                        var user = await _userManager.FindByEmailAsync(email);
                        if (user != null)
                        {
                            template.AllowedUsers.Add(new TemplateAccess
                            {
                                UserId = user.Id,
                                AccessType = "View"
                            });
                        }
                    }
                }

                await _templateService.CreateTemplateAsync(template);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // Shows the template edit form
        public async Task<IActionResult> Edit(Guid id)
        {
            var template = await _templateService.GetTemplateByIdAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            if (template.UserId != userId && !isAdmin)
            {
                return Forbid();
            }

            var viewModel = new EditTemplateViewModel
            {
                Id = template.Id,
                Title = template.Title,
                Description = template.Description,
                Topic = template.Topic,
                xmin = template.xmin,
                Questions = template.Questions.Select(q => new EditQuestionViewModel
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    Type = q.Type,
                    AllowEmpty = q.AllowEmpty,
                    Order = q.Order
                }).ToList()
            };

            return View(viewModel);
        }

        // Handles template edit form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditTemplateViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (!ModelState.IsValid) return View(viewModel);

            try
            {
                var existingTemplate = await _templateService.GetTemplateByIdAsync(id);
                if (existingTemplate == null) return NotFound();

                existingTemplate.Title = viewModel.Title;
                existingTemplate.Description = viewModel.Description;
                existingTemplate.Topic = viewModel.Topic;
                existingTemplate.xmin = viewModel.xmin;

                // Get existing question IDs
                var existingQuestionIds = existingTemplate.Questions.Select(q => q.Id).ToList();
                var submittedQuestionIds = viewModel.Questions.Select(q => q.Id).ToList();

                // Find questions to remove (existing questions not in submitted list)
                var questionsToRemove = existingTemplate.Questions
                    .Where(q => !submittedQuestionIds.Contains(q.Id))
                    .ToList();

                // Remove questions that are no longer present
                foreach (var question in questionsToRemove)
                {
                    existingTemplate.Questions.Remove(question);
                }

                // Update existing questions and add new ones
                foreach (var vmQuestion in viewModel.Questions)
                {
                    var existingQuestion = existingTemplate.Questions.FirstOrDefault(q => q.Id == vmQuestion.Id);
                    if (existingQuestion != null)
                    {
                        // Update existing question
                        existingQuestion.Title = vmQuestion.Title;
                        existingQuestion.Description = vmQuestion.Description;
                        existingQuestion.Type = vmQuestion.Type;
                        existingQuestion.AllowEmpty = vmQuestion.AllowEmpty;
                        existingQuestion.Order = vmQuestion.Order;
                    }
                    else
                    {
                        // Add new question
                        existingTemplate.Questions.Add(new Question
                        {
                            Title = vmQuestion.Title,
                            Description = vmQuestion.Description,
                            Type = vmQuestion.Type,
                            AllowEmpty = vmQuestion.AllowEmpty,
                            Order = vmQuestion.Order,
                            TemplateId = existingTemplate.Id
                        });
                    }
                }

                await _templateService.UpdateTemplateAsync(existingTemplate);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _templateService.TemplateExistsAsync(viewModel.Id))
                {
                    return NotFound();
                }
                else
                {
                    ModelState.AddModelError("", "The template was modified by another user. Please try again.");
                    return View(viewModel);
                }
            }
        }

        // Shows the template delete confirmation
        public async Task<IActionResult> Delete(Guid id)
        {
            var template = await _templateService.GetTemplateByIdAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (template.UserId != userId && !isAdmin)
            {
                return Forbid();
            }

            return View(template);
        }

        // Handles confirmed template deletion
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var template = await _templateService.GetTemplateByIdAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (template.UserId != userId && !isAdmin)
            {
                return Forbid();
            }

            await _templateService.DeleteTemplateAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Handles AJAX request for reordering questions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderQuestions(Guid templateId, List<Guid> questionOrder)
        {
            await _templateService.ReorderQuestions(templateId, questionOrder);
            return Ok();
        }

        // Shows all responses for a specific template
        [HttpGet]
        public async Task<IActionResult> Responses(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var template = await _templateService.GetTemplateByIdAsync(id);

            if (template == null) return NotFound();
            if (template.UserId != userId && !isAdmin) return Forbid();

            var responses = await _formService.GetFormsByTemplate(id);
            ViewBag.TemplateTitle = template.Title;

            return View(responses);
        }
    }
}