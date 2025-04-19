using ActiLink.Shared.Repositories;
using ActiLink.Shared.ServiceUtils;
using Microsoft.AspNetCore.Identity;

namespace ActiLink.Organizers.BusinessClients.Service
{
    public class BusinessClientService : IBusinessClientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Organizer> _userManager;

        public BusinessClientService(IUnitOfWork unitOfWork, UserManager<Organizer> userManager)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }


        /// <summary>
        /// Creates a new business client with the specified <paramref name="username"/>, <paramref name="email"/>, <paramref name="password"/> and <paramref name="taxId"/>.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="taxId"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="GenericServiceResult{T}"/> of the operation
        /// with the created <see cref="BusinessClient"/> object or null and error messages if the creation failed.
        /// </returns>
        public async Task<GenericServiceResult<BusinessClient>> CreateBusinessClientAsync(string username, string email, string password, string taxId)
        {
            var businessClient = new BusinessClient(username, email, taxId);
            var result = await _userManager.CreateAsync(businessClient, password);

            return result.Succeeded 
                ? GenericServiceResult<BusinessClient>.Success(businessClient) 
                : GenericServiceResult<BusinessClient>.Failure(result.Errors.Select(e => e.Description));
        }

        /// <summary>
        /// Retrieves all business clients from the database.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing an <see cref="IEnumerable{T}"/> of <see cref="BusinessClient"/> objects.
        /// </returns>
        public async Task<IEnumerable<BusinessClient>> GetBusinessClientsAsync()
        {
            return await _unitOfWork.BusinessClientRepository.GetAllAsync();
        }


        /// <summary>
        /// Finds and returns a business client, if any, by the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="BusinessClient"/> object if found, or null.
        /// </returns>
        public async Task<BusinessClient?> GetBusinessClientByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id) as BusinessClient;
        }

        /// <summary>
        /// Deletes the specified <paramref name="businessClient"/> from the database if exists.
        /// </summary>
        /// <param name="businessClient"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing a <see cref="ServiceResult"/> indicating success or failure.
        /// </returns>
        public async Task<ServiceResult> DeleteBusinessClientAsync(BusinessClient businessClient)
        {
            var result = await _userManager.DeleteAsync(businessClient);

            return result.Succeeded
                ? ServiceResult.Success()
                : ServiceResult.Failure(result.Errors.Select(e => e.Description));
        }
    }
}
