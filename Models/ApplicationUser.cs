// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using FormsApp.Features.Forms.Models;
using FormsApp.Features.Templates.Models;
using FormsApp.Features.Social.Models;
using System.ComponentModel.DataAnnotations;


namespace FormsApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }

        public string? ProfilePictureUrl { get; set; } = "/images/default-avatar.png";

        // User preferences
        public string Language { get; set; } = "en"; // Default to English
        public string Theme { get; set; } = "light"; // Default to light theme

        // Navigation properties
        public ICollection<Template> Templates { get; set; } = new List<Template>();
        public ICollection<Form> Forms { get; set; } = new List<Form>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<TemplateAccess> TemplateAccesses { get; set; } = new List<TemplateAccess>();
    }
}