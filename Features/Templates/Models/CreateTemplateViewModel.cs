using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FormsApp.Features.Templates.Models
{
    public class CreateTemplateViewModel
    {
        [Required]
        [StringLength(200)]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public string Topic { get; set; } = "Other";

        [Required]
        [MinLength(1, ErrorMessage = "At least 1 question is required")]
        [MaxLength(4, ErrorMessage = "Maximum 4 questions allowed")]
        public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();

        [Required]
        [Display(Name = "Template Visibility")]
        public string Visibility { get; set; } = "public"; // "public" or "private"

        [Display(Name = "Allowed Users (comma-separated emails)")]
        public string? AllowedUserEmails { get; set; }

        [Display(Name = "Tags (comma-separated)")]
        public string? Tags { get; set; }
    }

    public class QuestionViewModel
    {
        [Required(ErrorMessage = "Question title is required")]
        [StringLength(200)]
        public required string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Question type is required")]
        public required string Type { get; set; }

        public bool AllowEmpty { get; set; }
        public int Order { get; set; }
    }
}