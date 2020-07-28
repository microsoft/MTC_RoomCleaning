using RoomCleanup.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace RoomCleaning.AdminUI.Services
{
    public class SubscriptionService
    {
        private readonly HttpClient _client;

        public SubscriptionService(HttpClient client)
        {
            _client = client;
        }

        public async Task<ResourceData[]> GetRoomsAsync()
        {
            var rooms = new List<ResourceData>();
            for (int i = 0; i < 3; i++)
            {
                var room = new ResourceData() { Id = $"{i}" };
                rooms.Add(room);
            }

            return rooms.ToArray();
        }
    }
}
