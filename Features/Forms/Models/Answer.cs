using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FormsApp.Features.Social.Models;
using FormsApp.Features.Templates.Models;
using FormsApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FormsApp.Features.Forms.Models
{
    public class FormAnswer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid FormId { get; set; }

        [Required]
        public Guid QuestionId { get; set; }

        [Required]
        public string Value { get; set; }

        [ForeignKey("FormId")]
        public virtual Form Form { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    }
} 