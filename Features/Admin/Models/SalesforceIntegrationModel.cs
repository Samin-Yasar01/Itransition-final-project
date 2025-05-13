using System.ComponentModel.DataAnnotations;

namespace FormsApp.Features.Admin.Models
{
    public class SalesforceIntegrationModel
    {
        [Required]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Required]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; }

        [Display(Name = "Industry")]
        public string Industry { get; set; }

        [Display(Name = "Annual Revenue")]
        public decimal? AnnualRevenue { get; set; }
    }
}