using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctionAppDemo
{
    public static class HelloWorld
    {
        [FunctionName("HelloWorld")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/HttpTriggerCSharp/HelloWorld/{name}")]HttpRequestMessage req,
            string name, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            return req.CreateResponse($"Hello, {name}!");
        }
    }
}
