using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Security.Cryptography;
using System.Text;

namespace GeoExpande.Api.Services;

public sealed class EmailVerificationService(IConfiguration configuration)
{
    public static string CreateCode() => RandomNumberGenerator.GetInt32(0, 10_000).ToString("D4");

    public static string HashCode(string code) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

    public async Task SendCodeAsync(string email, string name, string code, CancellationToken cancellationToken = default)
    {
        var settings = configuration.GetSection("Email");
        var host = settings["SmtpHost"];
        var from = settings["From"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from)
            || !int.TryParse(settings["SmtpPort"], out var port)
            || string.IsNullOrWhiteSpace(settings["Username"])
            || string.IsNullOrWhiteSpace(settings["Password"]))
        {
            throw new InvalidOperationException("El servicio de correo no está configurado.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings["FromName"] ?? "GeoExpande", from));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "Verifica tu correo de GeoExpande";
        message.Body = new TextPart("plain")
        {
            Text = $"Hola {name},\n\nTu clave de verificación es: {code}\n\nLa clave vence en 10 minutos. Si no solicitaste esta cuenta, puedes ignorar este mensaje."
        };

        if (!Enum.TryParse<SecureSocketOptions>(settings["SecurityMode"] ?? "SslOnConnect", true, out var securityMode))
            throw new InvalidOperationException("La opción Email:SecurityMode no es válida.");

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, securityMode, cancellationToken);
        await client.AuthenticateAsync(settings["Username"]!, settings["Password"]!, cancellationToken);
        await client.SendAsync(new SmtpSendRequest(message), cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    public async Task SendPasswordResetCodeAsync(string email, string name, string code, CancellationToken cancellationToken = default)
    {
        var settings = configuration.GetSection("Email");
        var host = settings["SmtpHost"];
        var from = settings["From"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from)
            || !int.TryParse(settings["SmtpPort"], out var port)
            || string.IsNullOrWhiteSpace(settings["Username"])
            || string.IsNullOrWhiteSpace(settings["Password"]))
        {
            throw new InvalidOperationException("El servicio de correo no está configurado.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings["FromName"] ?? "GeoExpande", from));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "Restablece tu contraseña de GeoExpande";
        message.Body = new TextPart("plain")
        {
            Text = $"Hola {name},\n\nTu código para restablecer la contraseña es: {code}\n\nEl código vence en 10 minutos. Si no solicitaste este cambio, puedes ignorar este mensaje."
        };

        if (!Enum.TryParse<SecureSocketOptions>(settings["SecurityMode"] ?? "SslOnConnect", true, out var securityMode))
            throw new InvalidOperationException("La opción Email:SecurityMode no es válida.");

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, securityMode, cancellationToken);
        await client.AuthenticateAsync(settings["Username"]!, settings["Password"]!, cancellationToken);
        await client.SendAsync(new SmtpSendRequest(message), cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
