using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using Task = System.Threading.Tasks.Task;
namespace BidUp.BusinessLogic.Services;

public class BrevoEmailService : IEmailService
{
    public async Task SendConfirmationEmail(string userEmail, string confirmationLink)
    {
        Configuration.Default.ApiKey["api-key"] = Environment.GetEnvironmentVariable("BREVO_EMAIL_SERVICE_API_KEY");
        var apiInstance = new TransactionalEmailsApi();

        var to = new List<SendSmtpEmailTo> { new(userEmail) };

        var sendSmtpEmail = new SendSmtpEmail(templateId: 3, to: to, _params: new { ConfirmationLink = confirmationLink });

        await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
    }

    public async Task SendPasswordResetEmail(string userEmail, string passowrdResetPageLink)
    {
        Configuration.Default.ApiKey["api-key"] = Environment.GetEnvironmentVariable("BREVO_EMAIL_SERVICE_API_KEY");
        var apiInstance = new TransactionalEmailsApi();

        var to = new List<SendSmtpEmailTo> { new(userEmail) };

        var sendSmtpEmail = new SendSmtpEmail(templateId: 5, to: to, _params: new { PassowrdResetPageLink = passowrdResetPageLink });

        await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
    }
}