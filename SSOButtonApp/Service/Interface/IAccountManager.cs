﻿using Microsoft.AspNetCore.Authentication;
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
        Task<ExternalLoginInfo> GetExternalLoginInfoAsync();
        Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent);
        Task<IdentityResult> CreateUserExternally(ApplicationUser user);
        Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo info);
        AuthenticationProperties ConfigureExternalAuthenticationProperties(string redirectUrl);
    }
}
