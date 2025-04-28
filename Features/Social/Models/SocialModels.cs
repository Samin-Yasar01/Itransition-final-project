// In Features/Social/Models/SocialModels.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormsApp.Features.Forms.Models;
using FormsApp.Features.Templates.Models;
using FormsApp.Models;

namespace FormsApp.Features.Social.Models
{
    public class Comment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? TemplateId { get; set; }
        public Guid? AnswerId { get; set; } // Add this

        [Required]
        public string UserId { get; set; }

        [ForeignKey("TemplateId")]
        public virtual Template Template { get; set; }

        [ForeignKey("AnswerId")]
        public virtual FormAnswer Answer { get; set; } // Add this

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }

    public class Like
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? TemplateId { get; set; }
        public Guid? AnswerId { get; set; } // Add this

        [Required]
        public string UserId { get; set; }

        [ForeignKey("TemplateId")]
        public virtual Template Template { get; set; }

        [ForeignKey("AnswerId")]
        public virtual FormAnswer Answer { get; set; } // Add this

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}