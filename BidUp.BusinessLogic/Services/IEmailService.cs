namespace BidUp.BusinessLogic.Services;

public interface IEmailService
{
    Task SendConfirmationEmail(string userEmail, string confirmationLink);
    Task SendPasswordResetEmail(string userEmail, string passowrdResetPageLink);
}
