using Microsoft.Extensions.Configuration;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using Task = System.Threading.Tasks.Task;
namespace BidUp.BusinessLogic.Services;

public class BrevoEmailService : IEmailService
{
    private readonly IConfiguration configuration;
    private readonly TransactionalEmailsApi apiInstance;
    public BrevoEmailService(IConfiguration configuration)
    {
        Configuration.Default.ApiKey["api-key"] = Environment.GetEnvironmentVariable("BREVO_EMAIL_SERVICE_API_KEY");
        apiInstance = new TransactionalEmailsApi();
        this.configuration = configuration;
    }

    public async Task SendConfirmationEmail(string userEmail, string confirmationLink)
    {
        var to = new List<SendSmtpEmailTo> { new(userEmail) };

        var emailTemplateId = long.Parse(configuration["BrevoEmailApi:ConfirmationEmailTemplateId"]!);

        var sendSmtpEmail = new SendSmtpEmail(templateId: emailTemplateId, to: to, _params: new { ConfirmationLink = confirmationLink });

        await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
    }

    public async Task SendPasswordResetEmail(string userEmail, string passowrdResetPageLink)
    {
        var to = new List<SendSmtpEmailTo> { new(userEmail) };

        var emailTemplateId = long.Parse(configuration["BrevoEmailApi:PasswordResetEmailTemplateId"]!);

        var sendSmtpEmail = new SendSmtpEmail(templateId: emailTemplateId, to: to, _params: new { PassowrdResetPageLink = passowrdResetPageLink });

        await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
    }
}