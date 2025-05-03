using ActiLink.Configuration;
using ActiLink.Organizers.Authentication;
using ActiLink.Organizers.Authentication.Tokens;
using ActiLink.Shared.Repositories;
using ActiLink.Shared.ServiceUtils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ActiLink.Organizers.BusinessClients.Service
{
    public class BusinessClientService : IBusinessClientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Organizer> _userManager;
        private readonly IJwtTokenProvider _tokenProvider;
        private readonly JwtSettings _jwtSettings;

        private static readonly string[] InvalidLoginError = ["Invalid email or password."];
        private static readonly string[] FailedRefreshTokenSave = ["Failed to save the refresh token."];
        private static readonly string[] BusinessCLientNotFoundError = ["Business client not found."];

        public BusinessClientService(IUnitOfWork unitOfWork, UserManager<Organizer> userManager, IJwtTokenProvider provider, IOptions<JwtSettings> jwtOptions)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tokenProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            _jwtSettings = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
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
        /// Authenticates a user with the specified <paramref name="email"/> and <paramref name="password"/>.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="GenericServiceResult{T}"/> of the operation 
        /// with the access and refresh tokens or null and error messages if the authentication failed.
        /// </returns>
        public async Task<GenericServiceResult<(string AccessToken, string RefreshToken)>> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return GenericServiceResult<(string, string)>.Failure(InvalidLoginError);

            var result = await _userManager.CheckPasswordAsync(user, password);

            if (!result)
                return GenericServiceResult<(string, string)>.Failure(InvalidLoginError);

            var accessToken = _tokenProvider.GenerateAccessToken(user);
            var refreshToken = _tokenProvider.GenerateRefreshToken(user.Id);

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                ExpiryTimeUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                TokenOwner = user
            };

            await _unitOfWork.RefreshTokenRepository.AddAsync(refreshTokenEntity);
            var saveResult = await _unitOfWork.SaveChangesAsync();
            if (saveResult == 0)
                return GenericServiceResult<(string, string)>.Failure(FailedRefreshTokenSave);

            return GenericServiceResult<(string, string)>.Success((accessToken, refreshToken));
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

        /// <summary>
        /// Seeks and updates buisness client based on the specified <paramref name="id"/> and <paramref name="updateBusinessClientObject"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateBusinessClientObject"></param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="GenericServiceResult{T}"/> of the operation
        /// </returns>
        public async Task<GenericServiceResult<BusinessClient>> UpdateBusinessClientAsync(string id, UpdateBusinessClientObject updateBusinessClientObject)
        {
            var businessClient = await GetBusinessClientByIdAsync(id);
            if (businessClient == null)
                return GenericServiceResult<BusinessClient>.Failure(BusinessCLientNotFoundError);

            var result = await _userManager.SetUserNameAsync(businessClient, updateBusinessClientObject.Name);
            if (!result.Succeeded)
                return GenericServiceResult<BusinessClient>.Failure(result.Errors.Select(e => e.Description), ErrorCode.ValidationError);

            result = await _userManager.SetEmailAsync(businessClient, updateBusinessClientObject.Email);
            if (!result.Succeeded)
                return GenericServiceResult<BusinessClient>.Failure(result.Errors.Select(e => e.Description), ErrorCode.ValidationError);


            businessClient.TaxId = updateBusinessClientObject.TaxId;
            result = await _userManager.UpdateAsync(businessClient);

            return result.Succeeded ? GenericServiceResult<BusinessClient>.Success(businessClient) : GenericServiceResult<BusinessClient>.Failure(result.Errors.Select(e => e.Description));
        }
    }
}
