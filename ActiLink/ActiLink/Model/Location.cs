using Microsoft.EntityFrameworkCore;

namespace ActiLink.Model
{
    [Owned]
    public class Location
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public Location() { }

        public Location(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
    }
}
