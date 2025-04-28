using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormsApp.Features.Forms.Models;
using FormsApp.Features.Social.Models;
using FormsApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FormsApp.Features.Templates.Models
{
    public class Template
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public string Topic { get; set; } = "Other";

        [Required]
        public string UserId { get; set; }

        

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public bool IsPublic { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<Form> Forms { get; set; } = new List<Form>();
        public virtual ICollection<TemplateAccess> AllowedUsers { get; set; } = new List<TemplateAccess>();
        public virtual ICollection<TemplateTag> TemplateTags { get; set; } = new List<TemplateTag>();

        // Add these social relationships
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

        // Helper methods for question type validation
        public bool CanAddQuestionType(string type)
        {
            var count = Questions.Count(q => q.Type == type);
            return count < 4;
        }

        public int GetQuestionTypeCount(string type)
        {
            return Questions.Count(q => q.Type == type);
        }
    }

    public class Question
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TemplateId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string Type { get; set; } // "Text", "Multiline", "Number", "Checkbox"

        public bool AllowEmpty { get; set; } = false;

        [Required]
        public int Order { get; set; }

        [ForeignKey("TemplateId")]
        public virtual Template Template { get; set; }
    }

    public class TemplateAccess
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TemplateId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string AccessType { get; set; }

        [ForeignKey("TemplateId")]
        public virtual Template Template { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }

    public class Tag
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public virtual ICollection<TemplateTag> TemplateTags { get; set; } = new List<TemplateTag>();
    }

    public class TemplateTag
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TemplateId { get; set; }

        [Required]
        public Guid TagId { get; set; }

        [ForeignKey("TemplateId")]
        public virtual Template Template { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }
    }
}