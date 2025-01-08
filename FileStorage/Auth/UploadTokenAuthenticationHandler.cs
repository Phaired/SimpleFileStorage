using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace FileStorage.Auth;

public class UploadTokenAuthenticationHandler 
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public UploadTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock
    ) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1) Lire la variable d'environnement UploadToken
        var uploadToken = Environment.GetEnvironmentVariable("UploadToken");
        
        // 2) Si elle est vide ou nulle => aucune auth requise pour l’upload
        if (string.IsNullOrEmpty(uploadToken))
        {
            // On autorise l'accès : on renvoie un "utilisateur" vide, mais authentifié
            var identity = new ClaimsIdentity(Array.Empty<Claim>(), Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        // 3) Vérifier la présence du header "Token"
        if (!Request.Headers.TryGetValue("Token", out var providedToken))
        {
            return Task.FromResult(
                AuthenticateResult.Fail("Missing 'Token' header for upload.")
            );
        }

        // 4) Comparer la valeur reçue avec la valeur attendue
        if (!string.Equals(providedToken, uploadToken, StringComparison.Ordinal))
        {
            return Task.FromResult(
                AuthenticateResult.Fail("Invalid UploadToken.")
            );
        }

        // 5) Authentification validée => construire un ticket d’auth
        var claims = new[] { new Claim("ApiAccess", "Upload") };
        var identity2 = new ClaimsIdentity(claims, Scheme.Name);
        var principal2 = new ClaimsPrincipal(identity2);
        var ticket2 = new AuthenticationTicket(principal2, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket2));
    }
}
