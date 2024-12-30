# Single Sign-On (SSO) Implementation in C# .NET Core Web Application

## Overview

This project demonstrates how to implement **Single Sign-On (SSO)** in a **C# .NET Core** web application. Single Sign-On allows users to authenticate once and gain access to multiple applications without the need to log in again for each service. This can be achieved using various methods, including a **button-based SSO** integration and a **non-button-based SSO** integration.

### Concepts

1. **SSO with a Button**:
   - The user clicks a login button to initiate the SSO flow. This button triggers the authentication process, redirecting the user to the identity provider (IdP) for authentication.
   - Once authenticated, the user is redirected back to the application with an authorization code or token that grants access.

2. **SSO without a Button**:
   - In this method, the authentication process is triggered automatically when a user accesses the application.
   - The application checks if the user is authenticated by the IdP and logs them in automatically if they are.
   - If the user is not authenticated, they are silently redirected to the IdP for login.

---

## Prerequisites

- **.NET Core SDK**: Ensure that you have the .NET Core SDK installed on your machine.
- **Identity Provider (IdP)**: This application assumes the use of an identity provider such as **Azure AD**, **Okta**, or **Auth0** for handling the authentication process.
- **Redirect URIs**: Make sure your IdP is configured to handle redirects to the appropriate URLs in your application.

---

## Installation

1. Clone the repository to your local machine:
   ```bash
   git clone https://github.com/Elanchezhiyan-P/SSOIntegrationSolution.git
   cd SSOIntegrationSolution 
   ```
2. Open the project in Visual Studio or your preferred IDE.
3. Update your appsettings.json with the correct values for your identity provider (Client ID, Client Secret, etc.):

```bash
{
  "Authentication": {
    "Authority": "https://your-idp.com",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "RedirectUri": "https://localhost:5001/signin-oidc"
  }
}
```

4. Run the application:

```
dotnet run
```

## SSO with Button Integration
In this implementation, the application will provide a login button that triggers the authentication process.

### How it works:
1. The user clicks the Login with SSO button.
2. The application redirects the user to the identity provider for authentication.
3. Upon successful authentication, the identity provider redirects the user back to the application with an authorization code.
4. The application exchanges the authorization code for an access token and grants access.

Example Implementation
In the Program.cs file, configure the application to use authentication:

```bash

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "Microsoft";
    }).AddMicrosoftAccount(options =>
    {
        options.ClientId = builder.Configuration["AzureAd:ClientId"];
        options.ClientSecret = builder.Configuration["AzureAd:ClientSecret"];
        options.AuthorizationEndpoint = $"https://login.microsoftonline.com/{tenantid}/oauth2/v2.0/authorize";
        options.TokenEndpoint = $"https://login.microsoftonline.com/{tenantid}/oauth2/v2.0/token";
        options.AccessDeniedPath = "/Account/AccessDenied";
    }).AddCookie(options =>
    {
        options.Cookie.Name = "SSOButtonApp";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
    });


```

### SSO Without Button Integration
This method automatically redirects users to the identity provider if they are not authenticated.

## How it works:
1. Upon accessing the application, it checks whether the user is authenticated.
2. If the user is not authenticated, they are silently redirected to the identity provider.
3. Once authenticated, the user is redirected back to the application.

Example Implementation
In the Program.cs file, add the automatic authentication handling:

```bash

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    });

```

In this case, there's no need for a login button; the authentication process will be triggered automatically when the user accesses any protected page.

### Troubleshooting
- Callback URL Issues: Ensure that your IdP's redirect URI matches the callback path set in your application (/signin-oidc).
- Invalid Client Credentials: Double-check the ClientId and ClientSecret in your appsettings.json file.
- Token Expiry: If you encounter issues related to token expiration, make sure your refresh tokens are set up properly to allow automatic renewal of authentication tokens.

### Conclusion
This project demonstrates the implementation of SSO in a C# .NET Core web application. Both button-based and automatic SSO flows are shown, allowing flexibility based on user experience preferences. This setup provides a secure, seamless authentication experience for users across multiple services or applications.
