using System;
using System.Collections.Generic;
using System.Text;

namespace RoomCleaning.Shared.Models
{
    public class RoomPolicy
    {
        public Room Room { get; set; }
        public CleaningPolicy Policy { get; set; }
    }
}
