using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormsApp.Features.Templates.Models;
using FormsApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FormsApp.Features.Forms.Models
{
    public class Form
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TemplateId { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsSubmitted { get; set; } = false;

        [ForeignKey("TemplateId")]
        public virtual Template Template { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<FormAnswer> Answers { get; set; } = new List<FormAnswer>();

        public virtual ICollection<QuestionSnapshot> QuestionSnapshots { get; set; } = new List<QuestionSnapshot>();
    }
} 