using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RoomCleaning.Shared.Models
{
    public enum NotificationType
    {
        Email,
        SMS
    }

    public class RoomPolicy
    {


        public int CleaningTime { get; set; } = 30;
        public bool CleanBefore { get; set; } = false;
        public NotificationType NotificationType { get; set; } = NotificationType.Email;
        public string NotificationAlias { get; set; } = string.Empty;
    }
}
