using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace FileStorage.Auth;

public class DownloadTokenAuthenticationHandler 
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DownloadTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock
    ) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1) Lire la variable d'environnement DownloadToken
        var downloadToken = Environment.GetEnvironmentVariable("DownloadToken");

        // 2) Si elle est vide => pas de protection pour le download
        if (string.IsNullOrEmpty(downloadToken))
        {
            var identity = new ClaimsIdentity(Array.Empty<Claim>(), Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        // 3) Vérifier la présence du header "Token"
        if (!Request.Headers.TryGetValue("Token", out var providedToken))
        {
            return Task.FromResult(
                AuthenticateResult.Fail("Missing 'Token' header for download.")
            );
        }

        // 4) Comparer
        if (!string.Equals(providedToken, downloadToken, StringComparison.Ordinal))
        {
            return Task.FromResult(
                AuthenticateResult.Fail("Invalid DownloadToken.")
            );
        }

        // 5) Authentification validée
        var claims = new[] { new Claim("ApiAccess", "Download") };
        var identity2 = new ClaimsIdentity(claims, Scheme.Name);
        var principal2 = new ClaimsPrincipal(identity2);
        var ticket2 = new AuthenticationTicket(principal2, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket2));
    }
}