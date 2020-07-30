using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using RoomCleaning.Shared.Models;

namespace RoomCleaning.API
{
    public static class RenewSubscriptions
    {
        [FunctionName("RenewSubscriptions")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ExecutionContext context, ILogger log)
        {
            log.LogInformation($"RenewSubscriptions Timer trigger function executed at: {DateTime.Now}");

            var config = Helper.GetConfig(context);

            int expirationCheck = Convert.ToInt32(config["SubscriptionExpirationCheck"]);
            int subscriptionLength = Convert.ToInt32(config["SubscriptionLength"]);

            var graphServiceClient = Helper.GetGraphClient(config);

            //TODO: get all subscriptions from DB
            Shared.Models.Subscription[] subs = new Shared.Models.Subscription[] { };

            foreach(Shared.Models.Subscription sub in subs)
            {
                // if the subscription expires in the next X min, renew it
                if (sub.Expiration < DateTime.UtcNow.AddMinutes(expirationCheck))
                {
                    RenewSubscription(sub, graphServiceClient, subscriptionLength);
                }
            }
        }

        private static async void RenewSubscription(Shared.Models.Subscription subscription, GraphServiceClient graphServiceClient, int subscriptionLength)
        {
            Console.WriteLine($"Current subscription: {subscription.Id}, Expiration: {subscription.Expiration}");

            //DateTimeOffset newExpiration = DateTime.UtcNow.AddMinutes(subscriptionLength);

            var updatedSubscription = new Microsoft.Graph.Subscription
            {
                ExpirationDateTime = DateTime.UtcNow.AddMinutes(subscriptionLength) //newExpiration
            };

            await graphServiceClient
              .Subscriptions[subscription.Id]
              .Request()
              .UpdateAsync(updatedSubscription);

            subscription.Expiration = updatedSubscription.ExpirationDateTime;// newExpiration;

            //TODO: update subscription in DB
            Console.WriteLine($"Renewed subscription: {subscription.Id}, New Expiration: {subscription.Expiration}");
        }
    }
}
