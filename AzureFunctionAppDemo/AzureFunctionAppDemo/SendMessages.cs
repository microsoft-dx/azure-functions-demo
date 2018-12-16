using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AzureFunctionAppDemo
{
    public static class SendMessages
    {
        private static string sendgridKey = Environment.GetEnvironmentVariable("sendgridKey");
        private static SendGridClient client = new SendGridClient(sendgridKey);

        [FunctionName("SendMessages")]
        public static async Task Run([QueueTrigger("nba-games-queue", Connection = "StorageConnectionString")]string queueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {queueItem}");
            var receiver = queueItem.Split(',')[0];
            var content = queueItem.Split(',')[1];

            await SendEmail(receiver, content);
        }

        static async Task SendEmail(string receiver, string content)
        {
            var from = new EmailAddress("NBAGamesNotification@NBAGamesNotification.com", "NBA Games Notification");
            var subject = "NBA Game tomorrow!";
            var to = new EmailAddress(receiver);
            var plainTextContent = content;
            var htmlContent = $"<strong>{content}</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
