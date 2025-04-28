using FormsApp.Models;

namespace FormsApp.Features.Admin.Models
{
    public class UserManagementModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBlocked { get; set; }
    }

    public class AdminActionModel
    {
        public string UserId { get; set; }
        public string Action { get; set; }  // "toggle-admin", "toggle-block", "delete"
    }
}