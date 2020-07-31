using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoomCleaning.Shared.Models
{
    public class Subscription
    {

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public DateTimeOffset? Expiration { get; set; }
        public RoomPolicy RoomPolicy { get; set; }
    }
}
