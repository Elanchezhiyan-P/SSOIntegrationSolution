using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SSOButtonApp.Models;
using System.Security.Claims;

namespace SSOButtonApp.Service.Interface
{
    public interface IAccountManager
    {
        bool IsUserSignedIn(ClaimsPrincipal User);
        Task<ApplicationUser> GetUserAsync (ClaimsPrincipal User);
        Task<List<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync();
        Task UpdateAsync(ApplicationUser user);
        Task<ApplicationUser> FindByEmailAsync(string email);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<SignInResult> SignInAsync(ApplicationUser user, string password, bool rememberMe, bool isExternal = false);
        Task SignOut();
    }
}
