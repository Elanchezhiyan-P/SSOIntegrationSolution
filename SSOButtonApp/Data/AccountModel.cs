using Microsoft.AspNetCore.Authentication;

namespace SSOButtonApp.Data
{
    public class LoginMethodModel
    {
        public string ReturnUrl { get; set; }
        public IEnumerable<AuthenticationScheme> LoginMethods { get; set; }
        public string GreetingMessage { get; set; }
        public bool IsBirthday { get; set; }
        public bool IsFirstLoginToday { get; set; }
    }
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
