using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace SimpleCRUD.Services.STMP;

public class EmailSender : IEmailSender
{ 
    public Task SendEmailAsync(string email, string subject, string message)
    {
        SmtpClient client = new SmtpClient
        {
            Host = "smtp-relay.sendinblue.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential("momahgoub172@gmail.com","Pwh9vVCc6jEfz4JY")
        };
        return client.SendMailAsync("momahgoub172@gmail.com", email , subject, message);
    }
}