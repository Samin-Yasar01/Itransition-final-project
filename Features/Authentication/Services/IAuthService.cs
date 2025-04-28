using FormsApp.Features.Authentication.Models;
using Microsoft.AspNetCore.Identity;

namespace FormsApp.Features.Authentication.Services
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterUserAsync(RegisterModel model);
        Task<SignInResult> LoginUserAsync(LoginModel model);
        Task LogoutUserAsync();
    }
}
