using System;

namespace WebApi.Models
{
    public class Launch
    {
        public string Id { get; set; }
        public string LaunchVehicle { get; set; }
        public string LaunchProvider { get; set; }
        public string Mission { get; set; }
        public DateTime LaunchDate { get; set; }
    }
}
