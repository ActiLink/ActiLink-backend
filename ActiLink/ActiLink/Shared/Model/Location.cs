using Microsoft.EntityFrameworkCore;

namespace ActiLink.Shared.Model
{
    [Owned]
    public class Location
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public Location() { }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
