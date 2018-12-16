using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Tweetinvi;
using Tweetinvi.Core.Extensions;
using Tweetinvi.Core.Parameters;

namespace AzureFunctionAppDemo
{
    public static class TwitterBot
    {
        private static CloudStorageAccount account = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("StorageConnectionString"));
        private static CloudBlobClient blobClient = account.CreateCloudBlobClient();
        private static CloudBlobContainer tweetsContainer = blobClient.GetContainerReference("twitter-bot");

        [FunctionName("TwitterBot")]
        public static void Run([TimerTrigger("0 0 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"TwitterBot azure function executed at: {DateTime.Now}");

            var randomTwitterIndexGenerator = new Random();
            var tweetIndex = randomTwitterIndexGenerator.Next(1, 100);
            var tweetBlobName = "Tweet_" + tweetIndex + ".txt";

            var blob = tweetsContainer.GetBlobReference(tweetBlobName);

            var tweetText = (string) null;
            using (var blobReadStream = new StreamReader(blob.OpenRead()))
            {
                tweetText = blobReadStream.ReadLine();
            }

            var consumerKey = Environment.GetEnvironmentVariable("consumerKey");
            var consumerSecret = Environment.GetEnvironmentVariable("consumerSecret");
            var userAccessToken = Environment.GetEnvironmentVariable("userAccessToken");
            var userAccessSecret = Environment.GetEnvironmentVariable("userAccessSecret");
            Auth.SetUserCredentials(consumerKey, consumerSecret, userAccessToken, userAccessSecret);

            var publishedTweet = Tweet.PublishTweet(tweetText);

            if (publishedTweet == null)
            {
                log.Error($"Failed to publish tweet. Sad! {tweetText}");
                throw new Exception($"Failed to publish tweet. Sad! {tweetText}");
            }
            else
            {
                log.Info($"Published tweet {publishedTweet.Id}");
            }
        }
    }
}
