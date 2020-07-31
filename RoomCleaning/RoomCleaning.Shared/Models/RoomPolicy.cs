using System;
using System.Collections.Generic;
using System.Text;

namespace RoomCleaning.Shared.Models
{
    public class RoomPolicy
    {
        public RoomDetail Room { get; set; }
        public CleaningPolicy CleaningPolicy { get; set; }
    }
}
