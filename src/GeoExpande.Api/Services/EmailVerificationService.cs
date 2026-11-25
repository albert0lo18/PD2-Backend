using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace GeoExpande.Api.Services;

public sealed class EmailVerificationService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
{
    private const string BrevoSendEmailUrl = "https://api.brevo.com/v3/smtp/email";

    public static string CreateCode() => RandomNumberGenerator.GetInt32(0, 10_000).ToString("D4");

    public static string HashCode(string code) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

    public Task SendCodeAsync(string email, string name, string code, CancellationToken cancellationToken = default) =>
        SendAsync(
            email,
            name,
            "Verifica tu correo de GeoExpande",
            $"Hola {name},\n\nTu clave de verificación es: {code}\n\nLa clave vence en 10 minutos. Si no solicitaste esta cuenta, puedes ignorar este mensaje.",
            cancellationToken);

    public Task SendPasswordResetCodeAsync(string email, string name, string code, CancellationToken cancellationToken = default) =>
        SendAsync(
            email,
            name,
            "Restablece tu contraseña de GeoExpande",
            $"Hola {name},\n\nTu código para restablecer la contraseña es: {code}\n\nEl código vence en 10 minutos. Si no solicitaste este cambio, puedes ignorar este mensaje.",
            cancellationToken);

    private async Task SendAsync(string email, string name, string subject, string textContent, CancellationToken cancellationToken)
    {
        var settings = configuration.GetSection("Email");
        var apiKey = settings["BrevoApiKey"];
        var from = settings["From"];
        var fromName = settings["FromName"] ?? "GeoExpande";
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException("El servicio de correo de Brevo no está configurado. Configura Email:BrevoApiKey y Email:From.");

        using var request = new HttpRequestMessage(HttpMethod.Post, BrevoSendEmailUrl);
        request.Headers.Add("api-key", apiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = JsonContent.Create(new BrevoEmailRequest(
            new BrevoEmailAddress(from, fromName),
            [new BrevoEmailAddress(email, name)],
            subject,
            textContent));

        var client = httpClientFactory.CreateClient();
        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Brevo rechazó el envío de correo (HTTP {(int)response.StatusCode} {response.StatusCode}): {responseBody}",
                null,
                response.StatusCode);
        }
    }

    private sealed record BrevoEmailAddress(string Email, string Name);
    private sealed record BrevoEmailRequest(
        BrevoEmailAddress Sender,
        IReadOnlyList<BrevoEmailAddress> To,
        string Subject,
        string TextContent);
}
