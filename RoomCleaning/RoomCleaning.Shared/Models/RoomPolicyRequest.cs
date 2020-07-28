using System;
using System.Collections.Generic;
using System.Text;

namespace RoomCleaning.Shared.Models
{
    public class RoomPolicyRequest
    {
        public Room[] Rooms { get; set; }
        public RoomPolicy Policy { get; set; }
    }
}
