using Newtonsoft.Json;
using RoomCleaning.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace RoomCleaning.AdminUI.Services
{
    public class RoomPolicyService
    {
        private readonly HttpClient _client;

        public RoomPolicyService(HttpClient client)
        {
            _client = client;
        }

        public async Task<Room[]> GetRoomsAsync()
        {
            var rooms = new List<Room>();
            for (int i = 0; i < 3; i++)
            {
                var room = new Room() { Id = $"{i}" , DisplayName = $"{i}Room"};
                rooms.Add(room);
            }

            return rooms.ToArray();
        }

        public async Task<bool> SendPolicyRequest(RoomPolicyRequest request)
        {
            if (request is null)
                throw new ArgumentNullException($"{nameof(request)} cannot be null");

            if (request.Policy is null || request.Rooms == null)
                throw new ArgumentNullException($"{nameof(request.Rooms)} and/or{nameof(request.Policy)} cannot be null");

            var json = JsonConvert.SerializeObject(request);

            return true;
        }

        public async Task<RoomPolicy[]> GetRoomPoliciesAsync()
        {
            var policies = new List<RoomPolicy>();
            for (int i = 0; i < 3; i++)
            {
                var policy = new RoomPolicy() { Policy = new CleaningPolicy(), Room = new Room { Id = $"{i}", DisplayName = $"{i} Room" } };
                policies.Add(policy);
            }

            return policies.ToArray();
        }
    }
}
