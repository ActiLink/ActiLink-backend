namespace ActiLink.Model
{
    public class Location
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public Location(int height, int width) 
        {
            Height = height;
            Width = width;
        }
    }
}
