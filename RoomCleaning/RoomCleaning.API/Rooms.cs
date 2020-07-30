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
using RoomCleaning.Shared.Models;
using System.Web.Http;

namespace RoomCleaning.API
{
    public static class Rooms
    {
        [FunctionName("Rooms")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("Rooms HTTP trigger function processed a request.");

            var config = Helper.GetConfig(context);

            try
            {
                var graphServiceClient = Helper.GetGraphClient(config);

                //TODO: need to page through results? https://docs.microsoft.com/en-us/graph/sdks/paging?tabs=csharp
                var rooms = await graphServiceClient
                    .Places["microsoft.graph.room"]
                    .Request()
                    //.Select(p => new { p.Id, p.DisplayName }) 
                    .GetAsync();

                //TODO: the id we get here is not the same as the user id we could/need use later

                //TODO: this can't be the best/right way to map the graph object to our object...
                string roomsJson = JsonConvert.SerializeObject(rooms);

                dynamic obj = JsonConvert.DeserializeObject(roomsJson);

                RoomDetail[] allRooms = JsonConvert.DeserializeObject<RoomDetail[]>(JsonConvert.SerializeObject(obj.value));

                return new OkObjectResult(allRooms);
            }
            catch (Exception ex)
            {
                return new InternalServerErrorResult();
            }
        }
    }
}
