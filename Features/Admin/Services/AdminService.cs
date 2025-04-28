using FormsApp.Data;
using FormsApp.Features.Admin.Models;
using FormsApp.Features.Forms.Models;
using FormsApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FormsApp.Features.Admin.Services
{
    public interface IAdminService
    {
        Task<List<UserManagementModel>> GetAllUsersAsync();
        Task<bool> ToggleAdminAsync(string userId);
        Task<bool> ToggleBlockAsync(string userId);
        Task<bool> DeleteUserAsync(string userId);
    }

    public class AdminService : IAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<List<UserManagementModel>> GetAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            var userModels = new List<UserManagementModel>();

            foreach (var user in users)
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                userModels.Add(new UserManagementModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsAdmin = isAdmin,
                    IsBlocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.Now
                });
            }

            return userModels;
        }

        public async Task<bool> ToggleAdminAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                if (!await _userManager.Users.AnyAsync(u => u.Id != userId && _userManager.IsInRoleAsync(u, "Admin").Result))
                    await _userManager.AddToRoleAsync(user, "Admin");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            return true;
        }

        public async Task<bool> ToggleBlockAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.Now)
                await _userManager.SetLockoutEndDateAsync(user, null);
            else
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

            return true;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Templates)
                    .ThenInclude(t => t.Comments)
                .Include(u => u.Templates)
                    .ThenInclude(t => t.Likes)
                .Include(u => u.Templates)
                    .ThenInclude(t => t.Forms)
                        .ThenInclude(f => f.Answers)
                .Include(u => u.Comments)
                .Include(u => u.Forms)
                    .ThenInclude(f => f.Answers)
                .Include(u => u.Likes)
                .Include(u => u.TemplateAccesses)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Delete template-related entities
                foreach (var template in user.Templates)
                {
                    _context.Comments.RemoveRange(template.Comments);
                    _context.Likes.RemoveRange(template.Likes);

                    foreach (var form in template.Forms)
                    {
                        _context.FormAnswers.RemoveRange(form.Answers);
                        _context.QuestionSnapshots.RemoveRange(form.QuestionSnapshots);
                        _context.Forms.Remove(form);
                    }

                    _context.Templates.Remove(template);
                }

                // Delete user's direct relationships
                _context.Comments.RemoveRange(user.Comments);
                _context.Likes.RemoveRange(user.Likes);
                _context.TemplateAccesses.RemoveRange(user.TemplateAccesses);

                // Delete user's forms and answers
                foreach (var form in user.Forms)
                {
                    _context.FormAnswers.RemoveRange(form.Answers);
                    _context.QuestionSnapshots.RemoveRange(form.QuestionSnapshots);
                    _context.Forms.Remove(form);
                }

                // Delete user
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded) throw new Exception("User deletion failed");

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}