using System;
using System.Collections.Generic;
using System.Text;

namespace RoomCleaning.Shared.Models
{
    public class MeetingNotification
    {
       // public string Id { get; set; }

        public RoomDetail Room { get; set; }

        public Meeting Meeting { get; set; }

        public Meeting Cleaning { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset Modified { get; set; }
    }
}
