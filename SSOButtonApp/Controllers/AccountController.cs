using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSOButtonApp.Data;
using SSOButtonApp.Helpers.Utils;
using SSOButtonApp.Models;
using SSOButtonApp.Service.Interface;
using System.Security.Claims;

namespace SSOButtonApp.Controllers
{
    public class AccountController(IAccountManager accountManager) : Controller
    {
        private readonly IAccountManager _accountManager = accountManager;

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            bool IsUserSignedIn = _accountManager.IsUserSignedIn(User);

            if (IsUserSignedIn)
            {
                return RedirectToAction(Constants.DASHBOARD_PATH);
            }
            else
            {
                string returnUrl = string.IsNullOrEmpty(Request.Query["returnUrl"]) ? string.Empty : Request.Query["returnUrl"].ToString();

                var user = await _accountManager.GetUserAsync(User);

                if (user != null)
                {
                    bool isFirstLoginToday = user.IsFirstLoginOfTheDay();

                    if (isFirstLoginToday)
                    {
                        user.LastLoginDate = DateTime.Now;
                        await _accountManager.UpdateAsync(user);
                    }
                }

                var loginMethod = new LoginMethodModel
                {
                    ReturnUrl = returnUrl,
                    LoginMethods = await _accountManager.GetExternalAuthenticationSchemesAsync(),
                };

                return View(loginMethod);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("An error occurred. Please try again!");

                var user = await _accountManager.FindByEmailAsync(login.Email);
                if (user == null)
                    return BadRequest("Invalid credentials!");

                bool checkPassword = await _accountManager.CheckPasswordAsync(user, login.Password);
                if (!checkPassword)
                    return BadRequest("Invalid credentials!");

                if (!user.IsActive)
                    return BadRequest($"User {user.FullName} is not active. Please contact Admin for more information!");

                var signInResult = await _accountManager.SignInAsync(user, login.Password, login.RememberMe);
                if (!signInResult.Succeeded)
                    return BadRequest("Some error has occurred in logging in. Please try again later!");

                //var userRoles = await _accountManager.GetRolesAsync(user);

                var principal = CreatePrincipal(user);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                {
                    IsPersistent = login.RememberMe
                });

                return Ok($"Welcome {user.FullName}! You have logged in successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest("Internal server error");
            }
        }

        private static ClaimsPrincipal CreatePrincipal(ApplicationUser user)
        {
            try
            {
                var authClaims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };

                var identity = new ClaimsIdentity(authClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                return new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                var authClaims = new List<Claim> { };
                var identity = new ClaimsIdentity(authClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                return new ClaimsPrincipal(identity);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await _accountManager.SignOut();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult ExternalLogin(string returnUrl = "/signin-microsoft")
        {
            var redirectUrl = Url.Action(action: "ExternalLoginCallback", controller: "Account", values: new { ReturnUrl = returnUrl });
            var properties = _accountManager.ConfigureExternalAuthenticationProperties(redirectUrl);
            return new ChallengeResult("Microsoft", properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl, string? remoteError)
        {
            returnUrl ??= Constants.DASHBOARD_PATH;

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View("Login");
            }

            var info = await _accountManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Error loading external login information.");
                return View("Login");
            }

            // Check if the user already has a login
            var signInResult = await _accountManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true);
            if (signInResult.Succeeded)
            {
                var emailClaim = info.Principal.FindFirst(ClaimTypes.Email);
                if (emailClaim != null)
                {
                    var email = emailClaim.Value;
                    var userInfo = await _accountManager.FindByEmailAsync(email);

                    if (userInfo != null && !userInfo.IsActive)
                    {
                        return RedirectToAction("AccessDenied", "Account");
                    }
                    var principal = CreatePrincipal(userInfo);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = true });
                    return LocalRedirect(returnUrl);
                }
            }

            var userEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (userEmail != null)
            {
                var user = await _accountManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    var dateOfBirthClaim = info.Principal.FindFirstValue("dob");  // Assuming the DOB is stored under the "dob" claim.
                    DateTime dateOfBirth;
                    if (!string.IsNullOrEmpty(dateOfBirthClaim) && DateTime.TryParse(dateOfBirthClaim, out dateOfBirth))
                    {
                    }
                    else
                    {
                        // Set to a default date if parsing fails or the claim is not available.
                        dateOfBirth = new DateTime(2000, 1, 1);  // Default value
                    }


                    var claims = info.Principal.Claims;
                    foreach (var claim in claims)
                    {
                        Console.WriteLine($"{claim.Type}: {claim.Value}");
                    }

                    user = new ApplicationUser
                    {
                        UserName = userEmail,
                        Email = userEmail,
                        FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                        LastName = info.Principal.FindFirstValue(ClaimTypes.Surname),
                        IsActive = true,
                        EmailConfirmed = true,
                        LockoutEnabled = false,
                        CreatedDt = DateTime.Now,
                        TwoFactorEnabled = false,
                        DateOfBirth = dateOfBirth,
                        LastLoginDate = DateTime.Now                        
                    };

                    await _accountManager.CreateUserExternally(user);
                }

                await _accountManager.AddLoginAsync(user, info);
                await _accountManager.SignInAsync(user, string.Empty, false, isExternal: true);

                var principal = CreatePrincipal(user);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = true });
                return LocalRedirect(returnUrl);
            }
            return View("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "Access Denied";
            return View();
        }
    }
}
