using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using SSOButtonApp.Models;
using SSOButtonApp.Service.Interface;
using System.Security.Claims;

namespace SSOButtonApp.Service.Repository
{
    public class AccountManager : IAccountManager
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountManager(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor, ILogger<AccountManager> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsUserSignedIn(ClaimsPrincipal user)
        {
            return _signInManager.IsSignedIn(user);
        }

        public async Task<ApplicationUser> GetUserAsync(ClaimsPrincipal User)
        {
            if (User == null) throw new ArgumentNullException("user");
            else if (!User.Identity.IsAuthenticated)
                return null;
            else
            {
                var email = (User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value) ?? throw new ArgumentException("No user has been found");
                var user = await _userManager.FindByEmailAsync(email);
                return user;
            }
        }

        public async Task<List<AuthenticationScheme>> GetExternalAuthenticationSchemesAsync()
        {
            var scheme = await _signInManager.GetExternalAuthenticationSchemesAsync();
            return scheme.ToList();
        }

        public async Task UpdateAsync(ApplicationUser user)
        {
            await _userManager.UpdateAsync(user);
        }

        public async Task<ApplicationUser> FindByEmailAsync(string email)
        {
            try
            {
                return await _userManager.FindByEmailAsync(email);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            try
            {
                return await _userManager.CheckPasswordAsync(user, password);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<SignInResult> SignInAsync(ApplicationUser user, string password, bool rememberMe, bool isExternal = false)
        {
            try
            {
                if (isExternal)
                {
                    await _signInManager.SignInAsync(user, rememberMe);
                    return null;
                }
                else
                {
                    return await _signInManager.PasswordSignInAsync(user, password, rememberMe, false);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task SignOut()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<ExternalLoginInfo> GetExternalLoginInfoAsync()
        {
            return await _signInManager.GetExternalLoginInfoAsync();
        }
        
        public async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent)
        {
            return await _signInManager.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent, bypassTwoFactor: true);
        }

        public async Task<IdentityResult> CreateUserExternally(ApplicationUser user)
        {
            try
            {
                return await _userManager.CreateAsync(user);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo info)
        {
            return await _userManager.AddLoginAsync(user, info);
        }
        
        public AuthenticationProperties ConfigureExternalAuthenticationProperties(string redirectUrl)
        {
            return _signInManager.ConfigureExternalAuthenticationProperties("Microsoft", redirectUrl);
        }
    }
}
