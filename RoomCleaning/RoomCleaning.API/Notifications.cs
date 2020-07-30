using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Extensions;
using Microsoft.AspNetCore.Mvc.Formatters.Internal;
using Microsoft.Graph;
using Microsoft.Azure.Documents.Client;

namespace RoomCleaning.API
{
    public static class Notifications
    {
        [FunctionName("Notifications")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log,
            [CosmosDB(
                databaseName: "RoomCleaning",
                collectionName: "Notifications",
                ConnectionStringSetting = "DatabaseConnection")] DocumentClient documentClient)
        {
            log.LogInformation("Notifications HTTP function processed a request.");

            // handle validation
            string validationToken = req.Query["validationToken"];

            if (!string.IsNullOrEmpty(validationToken))
            {
                log.LogInformation($"Received Token: '{validationToken}'");
                return new OkObjectResult(validationToken);
            }

            // handle notifications
            using (StreamReader reader = new StreamReader(req.Body))
            {
                var config = Helper.GetConfig(context);

                // config settings
                string cleanupSubject = config["CleaningMeetingSubject"];
                string cleanupBody = config["CleaningMeetingBody"];
                string cleanupEmailAddress = config["CleaningCrewEmailAddress"];
                //TODO: get duration and before/after from policy in DB
                int cleanupDuration = Convert.ToInt32(config["CleaningMeetingDuration"]);   // minutes
                bool cleanAfterMeeting = Convert.ToBoolean(config["CleaningMeetingAfter"]);  // true = after, false = before

                var graphServiceClient = Helper.GetGraphClient(config);

                string content = await reader.ReadToEndAsync();

                //Console.WriteLine(content);

                var notifications = JsonConvert.DeserializeObject<Shared.Models.Notifications>(content);

                foreach (var notification in notifications.Items)
                {
                    log.LogInformation($"Received notification: '{notification.Resource}', {notification.ResourceData?.Id}");

                    //TODO: figure out what to do with created/updated notification for created event to avoid duplicate room cleaning meetings
                    if (notification.ChangeType.ToLower().Equals("created"))
                    {
                        // make a graph call to get event details (we only get limited info in the notification)
                        var userId = notification.Resource.Split("/")[1];
                        var eventId = notification.Resource.Split("/")[3];
                        var calendarEvent = await graphServiceClient
                            .Users[userId].Events[eventId]
                            .Request()
                            .GetAsync();

                        log.LogInformation($"Event: '{calendarEvent.Subject}', Ends: '{calendarEvent.End.ToDateTime():s}'");

                        if (!calendarEvent.Subject.Equals(cleanupSubject))
                        {
                            // create cleanup event based on event from notification
                            var cleanup = new Microsoft.Graph.Event();
                            cleanup.Subject = cleanupSubject;
                            cleanup.Body = new Microsoft.Graph.ItemBody
                            {
                                ContentType = Microsoft.Graph.BodyType.Text,
                                Content = cleanupBody
                            };

                            cleanup.Attendees = new Attendee[] { new Attendee { EmailAddress = new EmailAddress { Address = cleanupEmailAddress }, Type = AttendeeType.Required } };

                            if (cleanAfterMeeting)
                            {
                                cleanup.Start = calendarEvent.End;
                                cleanup.End = calendarEvent.End.ToDateTime().AddMinutes(cleanupDuration).ToDateTimeTimeZone();
                            }
                            else
                            {
                                cleanup.Start = calendarEvent.Start.ToDateTime().AddMinutes(cleanupDuration * -1).ToDateTimeTimeZone();
                                cleanup.End = calendarEvent.Start;
                            }

                            // make graph call to create event
                            var newEvent = await graphServiceClient
                                .Users[userId].Events
                                .Request()
                                .AddAsync(cleanup);

                            //TODO: check newEvent

                            //TODO: store meeting/cleanup for tracking
                            Shared.Models.MeetingNotification meetingNotification = new Shared.Models.MeetingNotification
                            {
                                Room = new Shared.Models.RoomDetail
                                {
                                    Id = userId,
                                    DisplayName = newEvent.Organizer.EmailAddress.Name,
                                    EmailAddress = newEvent.Organizer.EmailAddress.Address
                                },
                                Meeting = new Shared.Models.Meeting
                                {
                                    Id = calendarEvent.Id,
                                    Start = calendarEvent.Start.ToDateTimeOffset(),
                                    End = calendarEvent.End.ToDateTimeOffset()
                                },
                                Cleaning = new Shared.Models.Meeting
                                {
                                    Id = newEvent.Id,
                                    Start = newEvent.Start.ToDateTimeOffset(),
                                    End = newEvent.End.ToDateTimeOffset()
                                },
                                Created = DateTime.UtcNow,
                                Modified = DateTime.UtcNow

                            };

                            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("RoomCleaning", "Notifications");
                            await documentClient.CreateDocumentAsync(collectionUri, meetingNotification);
                        }
                    }
                    else
                    {
                        log.LogInformation($"Skipped '{notification.ChangeType}' notification");
                    }
                }
            }

            log.LogInformation("END Notification");

            return new OkResult();
        }
    }
}
