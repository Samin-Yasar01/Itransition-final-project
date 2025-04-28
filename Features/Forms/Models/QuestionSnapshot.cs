using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormsApp.Features.Forms.Models
{
    public class QuestionSnapshot
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid FormId { get; set; }

        [Required]
        public Guid OriginalQuestionId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string Type { get; set; }

        public bool AllowEmpty { get; set; }

        [Required]
        public int Order { get; set; }

        [ForeignKey("FormId")]
        public virtual Form Form { get; set; }
    }
} 