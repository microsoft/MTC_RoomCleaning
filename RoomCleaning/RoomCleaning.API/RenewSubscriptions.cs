using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace RoomCleaning.API
{
    public static class RenewSubscriptions
    {
        [FunctionName("RenewSubscriptions")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"RenewSubscriptions Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
