using Store.Models.Interfaces.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Store.API.Services;

public class MailerSendEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MailerSendEmailService> _logger;

    public MailerSendEmailService(HttpClient httpClient, IConfiguration configuration, ILogger<MailerSendEmailService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken ct = default)
    {
        var apiKey = _configuration["MailerSend:ApiKey"];
        var fromEmail = _configuration["MailerSend:FromEmail"] ?? "no-reply@clexan.local";
        var fromName = _configuration["MailerSend:FromName"] ?? "ClexAn System";

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("MailerSend API Key is not configured. Email will not be sent to {Email}. Reset Link: {Link}", toEmail, resetLink);
            return;
        }

        var payload = new
        {
            from = new { email = fromEmail, name = fromName },
            to = new[] { new { email = toEmail } },
            subject = "Password Reset Request",
            text = $"You have requested a password reset. Click the link to reset your password: {resetLink}",
            html = $"<p>You have requested a password reset. Click the link below to reset your password:</p><p><a href=\"{resetLink}\">Reset Password</a></p>"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mailersend.com/v1/email")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        try
        {
            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Failed to send email via MailerSend. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
            }
            else
            {
                _logger.LogInformation("Password reset email sent to {Email}", toEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending email via MailerSend to {Email}", toEmail);
        }
    }
}
