using System;
using System.Collections.Generic;
using System.Text;

namespace RoomCleaning.Shared.Models
{
    public class Subscription
    {
        public string SubId { get; set; }
        public DateTimeOffset Expiration { get; set; }
        public RoomPolicy RoomPolicy { get; set; }
    }
}
