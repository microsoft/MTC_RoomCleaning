using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Microsoft.Azure.Documents.Client;
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
        public static void Run([TimerTrigger("0 */30 * * * *")]TimerInfo myTimer, ExecutionContext context, ILogger log,
            [CosmosDB(
                databaseName: "RoomCleaning",
                collectionName: "Subscriptions",
                ConnectionStringSetting = "DatabaseConnection",
                SqlQuery = "select * from Subscriptions")] IEnumerable<Shared.Models.Subscription> subscriptions,
            [CosmosDB(
                databaseName: "RoomCleaning",
                collectionName: "Subscriptions",
                ConnectionStringSetting = "DatabaseConnection")] DocumentClient documentClient)
        {
            log.LogInformation($"RenewSubscriptions Timer trigger function executed at: {DateTime.Now}");

            var config = Helper.GetConfig(context);

            int expirationCheck = Convert.ToInt32(config["SubscriptionExpirationCheck"]);
            int subscriptionLength = Convert.ToInt32(config["SubscriptionLength"]);

            var graphServiceClient = Helper.GetGraphClient(config);

            // loop through all subscriptions from DB
            foreach(Shared.Models.Subscription sub in subscriptions)
            {
                // if the subscription expires in the next X min, renew it
                if (sub.Expiration < DateTime.UtcNow.AddMinutes(expirationCheck))
                {
                    try
                    {
                        log.LogInformation($"Current subscription: {sub.Id}, Expiration: {sub.Expiration}");

                        DateTimeOffset newExpiration = DateTime.UtcNow.AddMinutes(subscriptionLength);
                        sub.Expiration = newExpiration; // replace expiration
                        // update graph
                        RenewSubscription(graphServiceClient, sub);
                        // update db
                        UpdateSubscription(documentClient, sub);

                        log.LogInformation($"Renewed subscription: {sub.Id}, New Expiration: {sub.Expiration}");
                    }
                    catch(Exception ex)
                    {
                        //TODO: need to deal with the renew graph call not working (this won't catch a failure)
                        log.LogInformation("Failed to renew subscription: " + ex.Message);
                    }
                }
            }
        }

        private static async void UpdateSubscription(DocumentClient documentClient, Shared.Models.Subscription subscription)
        {
            Uri documentUri = UriFactory.CreateDocumentUri("RoomCleaning", "Subscriptions", subscription.Id);
            await documentClient.ReplaceDocumentAsync(documentUri, subscription);
        }

        private static async void RenewSubscription(GraphServiceClient graphServiceClient, Shared.Models.Subscription subscription)
        {
            var updatedSubscription = new Microsoft.Graph.Subscription
            {
                ExpirationDateTime = subscription.Expiration
            };

            await graphServiceClient
                .Subscriptions[subscription.Id]
                .Request()
                .UpdateAsync(updatedSubscription);
        }
    }
}
