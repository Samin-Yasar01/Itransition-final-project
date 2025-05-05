using FormsApp.Data;
using FormsApp.Features.Forms.Models;
using FormsApp.Features.Forms.Services;
using FormsApp.Features.Templates.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FormsApp.Features.Forms.Controllers
{
    [Authorize]
    public class FormsController : Controller
    {
        private readonly IFormService _formService;
        private readonly ITemplateService _templateService;
        private readonly ApplicationDbContext _context;

        public FormsController(IFormService formService, ITemplateService templateService, ApplicationDbContext context)
        {
            _formService = formService;
            _templateService = templateService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            var forms = isAdmin 
                ? await _formService.GetAllFormsAsync() 
                : await _formService.GetUserForms(userId);
            
            return View(forms);
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid templateId)
        {
            var template = await _templateService.GetTemplateByIdAsync(templateId);
            if (template == null) return NotFound();

            var form = await _formService.CreateForm(templateId, User.FindFirstValue(ClaimTypes.NameIdentifier));
            return RedirectToAction(nameof(Edit), new { id = form.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var form = await _formService.GetFormWithAnswersAndSnapshots(id);
            if (form == null) return NotFound();

            return View(form);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, Form form)
        {
            try
            {
                var existingForm = await _formService.GetFormWithAnswersAndSnapshots(id);
                if (existingForm == null)
                {
                    TempData["ErrorMessage"] = "Form not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (existingForm.QuestionSnapshots == null || !existingForm.QuestionSnapshots.Any())
                {
                    TempData["ErrorMessage"] = "No questions found for this form.";
                    return RedirectToAction(nameof(Index));
                }

                // Clear existing answers
                if (existingForm.Answers != null)
                {
                    _context.FormAnswers.RemoveRange(existingForm.Answers);
                    existingForm.Answers.Clear();
                }

                // Process each question snapshot
                foreach (var question in existingForm.QuestionSnapshots.OrderBy(q => q.Order))
                {
                    var answerValue = Request.Form[$"Answers[{question.Order}].Value"].ToString();
                    
                    // For checkbox, handle the hidden input
                    if (question.Type == "Checkbox")
                    {
                        var checkboxValue = Request.Form[$"Answers[{question.Order}].Value"].ToString();
                        // If the checkbox was checked, it will have "true" value, otherwise use "false"
                        answerValue = checkboxValue.Contains("true") ? "true" : "false";
                    }

                    // Only add answer if it's not empty or if empty values are allowed
                    if (!string.IsNullOrEmpty(answerValue) || question.AllowEmpty)
                    {
                        var newAnswer = new FormAnswer
                        {
                            QuestionId = question.OriginalQuestionId,
                            Value = answerValue,
                            FormId = existingForm.Id
                        };
                        existingForm.Answers ??= new List<FormAnswer>();
                        existingForm.Answers.Add(newAnswer);
                        _context.FormAnswers.Add(newAnswer);
                    }
                }

                // Check if the form is being submitted
                if (Request.Form.ContainsKey("action"))
                {
                    var action = Request.Form["action"].ToString();
                    if (action == "submit")
                    {
                        existingForm.IsSubmitted = true;
                        TempData["SuccessMessage"] = "Your form has been submitted successfully!";
                        
                        // Save changes and redirect to View action for submitted forms
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(View), new { id = existingForm.Id });
                    }
                    else if (action == "draft")
                    {
                        TempData["SuccessMessage"] = "Your answers have been saved as draft.";
                    }
                }

                existingForm.UpdatedAt = DateTime.UtcNow;

                if (TryValidateModel(existingForm))
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Please correct the errors below");
                return View(existingForm);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while processing your request. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> View(Guid id)
        {
            var form = await _formService.GetFormWithAnswersAndSnapshots(id);
            if (form == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Authorization check
            if (form.UserId != userId && !isAdmin && !await _templateService.HasAccessToTemplate(form.TemplateId, userId))
            {
                return Forbid();
            }

            // Ensure answers are loaded
            if (form.Answers == null || !form.Answers.Any())
            {
                form.Answers = await _context.FormAnswers
                    .Where(a => a.FormId == form.Id)
                    .ToListAsync();
            }

            return View(form);
        }
    }
} 