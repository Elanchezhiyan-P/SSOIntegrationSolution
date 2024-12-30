using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSOButtonApp.Data;
using SSOButtonApp.Service.Interface;
using SSOButtonApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace SSOAutoLoginApp.Controllers
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
                return Ok(new { Message = "User has been signed in!", RedirectPath = "/Home/Dashboard" });

            }
            else
            {
                string returnUrl = string.IsNullOrEmpty(Request.Query["returnUrl"]) ? "/Home/Dashboard" : Request.Query["returnUrl"].ToString();

                var user = await _accountManager.GetUserAsync(User);

                if (user != null)
                {
                    bool isFirstLoginToday = user.IsFirstLoginOfTheDay();

                    if (isFirstLoginToday)
                    {
                        user.LastLoginDate = DateTime.Now;
                        await _accountManager.UpdateAsync(user);
                    }

                    var loginMethod = new LoginMethodModel
                    {
                        ReturnUrl = returnUrl,
                        LoginMethods = await _accountManager.GetExternalAuthenticationSchemesAsync(),
                        GreetingMessage = user.GreetMessage,
                        IsBirthday = user.IsBirthdayToday(),
                        IsFirstLoginToday = isFirstLoginToday
                    };

                    return Ok(loginMethod);
                }
                else
                {
                    return BadRequest("User is not found!");
                }
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
    }
}
