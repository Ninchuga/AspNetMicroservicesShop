using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace EmailFunction
{
    public class UploadEmailMessagesHttpTriggerFunction
    {
        [Function(nameof(UploadEmailMessagesHttpTriggerFunction))]
        [QueueOutput(Constants.EmailQueueName)] // stores return value to the specified queue
        public async Task<Email> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req,
            FunctionContext functionContext) // ILogger -> FunctionContext executionContext
        {
            ILogger logger = functionContext.GetLogger(nameof(UploadEmailMessagesHttpTriggerFunction));

            var request = await new StreamReader(req.Body).ReadToEndAsync();
            Email email = JsonConvert.DeserializeObject<Email>(request);

            return email;
        }
    }
}
