// The 'From' and 'To' fields are automatically populated with the values specified by the binding settings.
//
// You can also optionally configure the default From/To addresses globally via host.config, e.g.:
//
// {
//   "sendGrid": {
//      "to": "user@host.com",
//      "from": "Azure Functions <samples@functions.com>"
//   }
// }
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;

namespace EmailFunction
{
    public static class SendGridEmailQueueTriggerFunction
    {
        [Function(nameof(SendGridEmailQueueTriggerFunction))]
        [SendGridOutput()] // sends email which was built with SendGridMessage
        public static SendGridMessage Run(
            [QueueTrigger(Constants.EmailQueueName, Connection = "AzureWebJobsStorage")] Email email,
            FunctionContext context)
        {
            ILogger logger = context.GetLogger(nameof(SendGridEmailQueueTriggerFunction));

            SendGridMessage message = new();
            message.Subject = email.Subject;
            message.AddTo(email.CustomerEmail);
            message.AddContent("text/plain", email.Body);

            logger.LogInformation("Sending email for the customer {CustomerEmail}", email.CustomerEmail);

            return message;
        }
    }
}
