using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoomCleaning.Shared.Models
{
    public class Room
    {

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "geoCoordinates")]
        public string GeoCoordinates { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }

        [JsonProperty(PropertyName = "nickname")]
        public string Nickname { get; set; }

        [JsonProperty(PropertyName = "emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty(PropertyName = "building")]
        public string Building { get; set; }

        [JsonProperty(PropertyName = "floorNumber")]
        public string FloorNumber { get; set; }

        [JsonProperty(PropertyName = "floorLabel")]
        public string FloorLabel { get; set; }

        [JsonProperty(PropertyName = "label")]
        public string Label { get; set; }

        [JsonProperty(PropertyName = "capacity")]
        public string Capacity { get; set; }

        [JsonProperty(PropertyName = "bookingType")]
        public string BookingType { get; set; }

        [JsonProperty(PropertyName = "audioDeviceName")]
        public string AudioDeviceName { get; set; }

        [JsonProperty(PropertyName = "videoDeviceName")]
        public string VideoDeviceName { get; set; }

        [JsonProperty(PropertyName = "displayDeviceName")]
        public string DisplayDeviceName { get; set; }

        [JsonProperty(PropertyName = "isWheelChairAccessible")]
        public string IsWheelChairAccessible { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public string[] Tags { get; set; }
    }
}
