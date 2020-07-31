using Newtonsoft.Json;
using RoomCleaning.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RoomCleaning.TaskUI.Services
{
    public class CleaningScheduleService 
    {

        private HttpClient _client;

        public CleaningScheduleService(HttpClient client)
        {
            _client = client;
        }

        public async Task<Subscription[]> GetPoliciesAsync()
        {
            string output = "nothing";
            try
            {
                var result = await _client.GetAsync("/api/subscriptions");
                var json = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Subscription[]>(json);
            }
            catch (Exception ex)
            {
                output = ex.Message;
            }

            return new Subscription[0];
        }
    }
}
