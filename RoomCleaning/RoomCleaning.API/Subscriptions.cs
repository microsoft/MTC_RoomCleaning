using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RoomCleaning.Shared.Models;
using Microsoft.Extensions.Configuration;

namespace RoomCleaning.API
{
    public static class Subscriptions
    {
        [FunctionName("Subscriptions")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("Subscriptions HTTP trigger function processed a request.");

            using (StreamReader reader = new StreamReader(req.Body))
            {
                string content = await reader.ReadToEndAsync();
                var roomPolicyRequest = JsonConvert.DeserializeObject<RoomPolicyRequest>(content);

                var config = Helper.GetConfig(context);

                // config settings
                int subscriptionLength = Convert.ToInt32(config["SubscriptionLength"]); // https://docs.microsoft.com/en-us/graph/api/resources/subscription?view=graph-rest-beta

                var graphServiceClient = Helper.GetGraphClient(config);

                foreach (Room room in roomPolicyRequest.Rooms)
                {
                    var sub = new Microsoft.Graph.Subscription()
                    {
                        ChangeType = "created,updated,deleted",
                        NotificationUrl = config["notificationsUrl"],
                        Resource = $"/users/{room.Email}/events",    //TOOD: I'd like to use Id, but the Places/Room.Id is not the same as User.Id
                        ExpirationDateTime = DateTime.UtcNow.AddMinutes(subscriptionLength),
                        ClientState = "SecretClientState"
                    };

                    var newSubscription = await graphServiceClient
                      .Subscriptions
                      .Request()
                      .AddAsync(sub);

                        //TODO: check subscription response

                        //TODO: store room/subscription
                }
            }

            //return $"Subscribed. Id: {newSubscription.Id}, Expiration: {newSubscription.ExpirationDateTime}";

            return new OkResult();
        }
    }
}
