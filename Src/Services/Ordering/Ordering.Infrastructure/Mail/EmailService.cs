using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Ordering.Application.Contracts.Infrastrucutre;
using Ordering.Application.Models;
using Ordering.Domain.Entities;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Mail
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendMailFor(string customerEmail, string userName, Guid orderId)
        {
            _logger.LogInformation("Sending email for the created order {OrderId}", orderId);

            var emailFunctionUrl = _configuration["Azure:EmailFunctionUrl"];

            // Image/logo for the email can be retreived from blob storage
            var email = new EmailTemplate()
            {
                CustomerEmail = customerEmail,
                Body = $"Hi {userName}, your order #{orderId} is being processed!",
                Subject = $"Your order confirmation"
            };

            var httpClient = _httpClientFactory.CreateClient();
            var requestContent = new StringContent(JsonConvert.SerializeObject(email), Encoding.UTF8, "application/json");
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                response = await httpClient.PostAsync(emailFunctionUrl, requestContent);
            }
            catch (Exception ex)
            {
                _logger.LogError("Order {OrderId} for the customer {UserName} failed due to an error with the mail service: {ErrorMessage}", orderId, userName, ex.Message);
            }

            if (response.IsSuccessStatusCode)
                _logger.LogInformation("Email has been successfully sent to the customer {UserName}", userName);
            else
                _logger.LogError("Email was not sent to the customer {UserName}", userName);
        }

        public async Task<bool> SendEmail(EmailTemplate email)
        {
            var client = new SendGridClient(_emailSettings.ApiKey);

            var subject = email.Subject;
            var to = new EmailAddress(email.CustomerEmail);
            var emailBody = email.Body;

            var from = new EmailAddress
            {
                Email = _emailSettings.FromAddress,
                Name = _emailSettings.FromName
            };

            var sendGridMessage = MailHelper.CreateSingleEmail(from, to, subject, emailBody, emailBody);
            var response = await client.SendEmailAsync(sendGridMessage);

            

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted || response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("Email has been successfully sent to the customer {CustomerEmail}", email.CustomerEmail);
                return true;
            }

            _logger.LogError("Email was not sent to the customer {CustomerEmail}", email.CustomerEmail);
            return false;
        }
    }
}
