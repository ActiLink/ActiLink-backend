namespace ActiLink.Hobbies.Service
{
    public interface IHobbyService
    {
        public Task<IEnumerable<Hobby>> GetHobbiesAsync();
        public Task<Hobby?> GetHobbyByIdAsync(Guid id);
    }
}
