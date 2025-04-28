using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Add this

namespace FormsApp.Features.Templates.Models
{
    public class EditTemplateViewModel
    {
        public EditTemplateViewModel()
        {
            Questions = new List<EditQuestionViewModel>();
        }

        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string Topic { get; set; }

      

        public List<EditQuestionViewModel> Questions { get; set; }
    }
    public class EditQuestionViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Question title is required")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Question type is required")]
        public string Type { get; set; }

        public bool AllowEmpty { get; set; }

        public int Order { get; set; }
    }
}