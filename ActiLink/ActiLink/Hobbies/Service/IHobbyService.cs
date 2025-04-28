namespace ActiLink.Hobbies.Service
{
    public interface IHobbyService
    {
        public Task<IEnumerable<Hobby>> GetHobbiesAsync();
    }
}
