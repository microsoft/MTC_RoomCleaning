using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoomCleaning.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace RoomCleaning.AdminUI.Services
{
    public class RoomPolicyService
    {
        private readonly HttpClient _client;
        private readonly string _authKey;

        public string BaseUri => _authKey;

        public RoomPolicyService(HttpClient client, IConfiguration config)
        {
            _client = client;
            _authKey = config["authKey"];
            //_client.DefaultRequestHeaders.Add("x-functions-key", _config["authKey"]);
           
        }

        public async Task<RoomDetail[]> GetRoomsAsync()
        {
            RoomDetail[] rooms = new RoomDetail[0];
            try
            {
                var request = CreateRequest("api/rooms");

                var result = await _client.SendAsync(request);
                var status = result.StatusCode;
                var json = await result.Content.ReadAsStringAsync();
                rooms = JsonConvert.DeserializeObject<RoomDetail[]>(json);
            }
            catch (Exception ex)
            {

            }

            return rooms;
        }

        private HttpRequestMessage CreateRequest(string endpoint)
        {
            var request = new HttpRequestMessage() { Method = HttpMethod.Get, RequestUri = new Uri($"{_client.BaseAddress}{endpoint}") };
            request.Headers.Add("x-functions-key", _authKey);
            return request;
        }

        public async Task<bool> SendPolicyRequest(RoomPolicyRequest request)
        {
            if (request is null)
                throw new ArgumentNullException($"{nameof(request)} cannot be null");

            if (request.Policy is null || request.Rooms == null)
                throw new ArgumentNullException($"{nameof(request.Rooms)} and/or{nameof(request.Policy)} cannot be null");

            var json = JsonConvert.SerializeObject(request);
            var result = await _client.PostAsync("/api/subscriptions", new StringContent(json));
            return result.StatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task<RoomPolicy[]> GetRoomPoliciesAsync()
        {
            var policies = new List<RoomPolicy>();
            for (int i = 0; i < 3; i++)
            {
                var policy = new RoomPolicy() { CleaningPolicy = new CleaningPolicy(), Room = new RoomDetail { Id = $"{i}", DisplayName = $"{i} Room" } };
                policies.Add(policy);
            }

            return policies.ToArray();
        }
    }
}
