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

namespace RoomCleaning.API
{
    public static class Notifications
    {
        [FunctionName("Notifications")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log) //, [FromQuery] string validationToken = null
        {
            log.LogInformation("Notifications HTTP function processed a request.");

            Console.WriteLine("START POST");

            // handle validation
            string validationToken = req.Query["validationToken"];

            if (!string.IsNullOrEmpty(validationToken))
            {
                Console.WriteLine($"Received Token: '{validationToken}'");
                return new OkObjectResult(validationToken);
            }

            // handle notifications
            using (StreamReader reader = new StreamReader(req.Body))
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                // config settings
                string cleanupSubject = config["CleaningMeetingSubject"];
                string cleanupBody = config["CleaningMeetingBody"];
                string cleanupEmailAddress = config["CleaningCrewEmailAddress"];
                int cleanupDuration = Convert.ToInt32(config["CleaningMeetingDuration"]);   // minutes
                bool cleanAfterMeeting = Convert.ToBoolean(config["CleaningMeetingAfter"]);  // true = after, false = before

                var graphServiceClient = Helper.GetGraphClient(config);

                string content = await reader.ReadToEndAsync();

                //Console.WriteLine(content);

                var notifications = JsonConvert.DeserializeObject<Shared.Models.Notifications>(content);

                foreach (var notification in notifications.Items)
                {
                    Console.WriteLine($"Received notification: '{notification.Resource}', {notification.ResourceData?.Id}");

                    // make a graph call to get event details (we only get limited info in the notification)
                    var userId = notification.Resource.Split("/")[1];
                    var eventId = notification.Resource.Split("/")[3];
                    var calendarEvent = await graphServiceClient
                        .Users[userId].Events[eventId]
                        .Request()
                        .GetAsync();

                    Console.WriteLine($"Event: '{calendarEvent.Subject}', Ends: '{calendarEvent.End.ToDateTime():s}'");

                    if (!calendarEvent.Subject.Equals(cleanupSubject))
                    {
                        //TODO: figure out what to do with created/updated notification for created event to avoid duplicate room cleaning meetings
                        // create cleanup event based on event from notification
                        var cleanup = new Microsoft.Graph.Event();
                        cleanup.Subject = cleanupSubject;
                        cleanup.Body = new Microsoft.Graph.ItemBody
                        {
                            ContentType = Microsoft.Graph.BodyType.Text,
                            Content = cleanupBody
                        };

                        //TODO: invite cleanup alias
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
                    }
                }
            }

            Console.WriteLine("END POST");

            return new OkResult();
        }
    }
}
