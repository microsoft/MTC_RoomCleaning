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
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Graph;
using System.Collections.Generic;

namespace RoomCleaning.API
{
    public static class Subscriptions
    {
        [FunctionName("Subscriptions")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", "get", "delete", Route = null)] HttpRequest req,
            ExecutionContext context,
            [CosmosDB(
                databaseName: "RoomCleaning",
                collectionName: "Subscriptions",
                ConnectionStringSetting = "DatabaseConnection")] DocumentClient documentClient,//IAsyncCollector<object> db,
            ILogger log)
        {
            log.LogInformation("Subscriptions HTTP trigger function processed a request.");

            var config = Helper.GetConfig(context);

            if (req.Method.ToUpper() == "POST")
            {
                using (StreamReader reader = new StreamReader(req.Body))
                {
                    string content = await reader.ReadToEndAsync();
                    var roomPolicyRequest = JsonConvert.DeserializeObject<RoomPolicyRequest>(content);

                    // config settings
                    int subscriptionLength = Convert.ToInt32(config["SubscriptionLength"]); // https://docs.microsoft.com/en-us/graph/api/resources/subscription?view=graph-rest-beta

                    var graphServiceClient = Helper.GetGraphClient(config);

                    foreach (Shared.Models.Room room in roomPolicyRequest.Rooms)
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

                        //TODO: check that subscription was created successfully

                        // store subscription
                        Shared.Models.Subscription subscription = new Shared.Models.Subscription
                        {
                            Id = newSubscription.Id,
                            Expiration = newSubscription.ExpirationDateTime,
                            RoomPolicy = new RoomPolicy
                            {
                                Room = new Shared.Models.RoomDetail
                                {
                                    //Id = room.Id, //TODO: add it back when we have the user.Id, not the places/room.Id
                                    EmailAddress = room.Email,
                                    DisplayName = room.DisplayName
                                },
                                CleaningPolicy = roomPolicyRequest.Policy
                            }
                        };

                        Uri collectionUri = UriFactory.CreateDocumentCollectionUri("RoomCleaning", "Subscriptions");
                        await documentClient.CreateDocumentAsync(collectionUri, subscription);

                        //await db.AddAsync(subscription);

                        log.LogInformation($"Subscribed. Id: {newSubscription.Id}, Expiration: {newSubscription.ExpirationDateTime}");
                    }
                }

                return new OkResult();
            }
            else if (req.Method.ToUpper() == "GET")
            {
                // get subscriptions
                List<Shared.Models.Subscription> subscriptions = new List<Shared.Models.Subscription>();
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri("RoomCleaning", "Subscriptions");

                using (var queryable = documentClient.CreateDocumentQuery<Shared.Models.Subscription>(collectionUri)
                    .AsDocumentQuery())
                {
                    while (queryable.HasMoreResults)
                    {
                        foreach (Shared.Models.Subscription sub in await queryable.ExecuteNextAsync<Shared.Models.Subscription>())
                        {
                            subscriptions.Add(sub);
                        }
                    }
                }

                return new OkObjectResult(subscriptions.ToArray());
            }
            else if (req.Method.ToUpper() == "DELETE")
            {
                //TODO: delete subscriptions

                return new OkResult();

            }
            else
            {
                return new BadRequestResult();
            }
        }
    }
}
