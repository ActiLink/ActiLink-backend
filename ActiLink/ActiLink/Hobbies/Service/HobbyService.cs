using ActiLink.Shared.Repositories;

namespace ActiLink.Hobbies.Service
{
    public class HobbyService : IHobbyService
    {
        private readonly IUnitOfWork _unitOfWork;
        public HobbyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IEnumerable<Hobby>> GetHobbiesAsync()
        {
            return await _unitOfWork.HobbyRepository.GetAllAsync();
        }

        public async Task<Hobby?> GetHobbyByIdAsync(Guid id)
        {
            return await _unitOfWork.HobbyRepository.GetByIdAsync(id);
        }

    }
}
