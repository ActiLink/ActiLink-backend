namespace ActiLink.Model
{
    public class Location
    {
        public Guid Id { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        public Location(int height, int width) 
        {
            Id = Guid.NewGuid();
            Height = height;
            Width = width;
        }
    }
}
