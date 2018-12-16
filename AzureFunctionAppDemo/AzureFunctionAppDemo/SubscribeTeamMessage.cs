using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.IO;
using System.Text.RegularExpressions;
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureFunctionAppDemo
{
    public static class SubscribeTeamMessage
    {
        private static CloudStorageAccount account = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("StorageConnectionString"));
        private static CloudQueueClient queueClient = account.CreateCloudQueueClient();
        private static CloudQueue queue = queueClient.GetQueueReference("nba-games-queue");

        [FunctionName("SubscribeTeamMessage")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/HttpTriggerCSharp/TeamMessage/{email}/{team}/")]HttpRequestMessage req,
            string email, string team, TraceWriter log)
        {
            log.Info($"C# HTTP trigger function processed a request: {email} : {team}");

            var scheduleHtml = (string)null;
            using (WebClient client = new WebClient()) 
            {
                scheduleHtml = client.DownloadString($@"http://www.espn.com/nba/teams/printSchedule?team={team}&season=2019");
            }

            var line = (string)null;
            using (var sr = new StringReader(scheduleHtml))
            {
                sr.ReadLine();
                sr.ReadLine();

                while ((line = sr.ReadLine()) != null)
                {
                    Regex hourRegex = new Regex("([0-9]|10|11|12):[0-5][0-9]");
                    var matchHour = hourRegex.Match(line).Value;

                    Regex dateRegex = new Regex("[A-Z][a-z][a-z]. ((\\w{2})|(\\w{1}))");

                    var matchDate = dateRegex.Match(line).Value;

                    var gameRegex = new Regex("(at .*)|([A-Z][a-z][a-z][a-z].*)|(LA<)");
                    var matchGame = gameRegex.Match(line).Value.Substring(0, gameRegex.Match(line).Value.IndexOf("<"));

                    if (matchHour != null && matchDate != null && matchGame != null)
                    {
                        try
                        {
                            var sendTime = DateTime.ParseExact(matchDate + " 2019 " + matchHour, "MMM. d yyyy h:mm",
                                System.Globalization.CultureInfo.InvariantCulture).AddDays(-1);

                            if (sendTime > DateTime.UtcNow)
                            {
                                queue.AddMessage(new CloudQueueMessage($"{email},team plays {matchGame} - {matchDate} {matchHour} Eastern"), null, DateTime.UtcNow - sendTime);
                                log.Info($"{email},team plays {matchGame} - {matchDate} {matchHour} Eastern");
                            }
                        }
                        catch
                        {
                            log.Warning($"Didn't process correctly: {line}");
                        }

                    }
                }
            }

            return req.CreateResponse($"Added {email} to 2019 season queue for team {team}");
        }
    }
}
