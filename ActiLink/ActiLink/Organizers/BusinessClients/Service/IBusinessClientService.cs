using ActiLink.Shared.ServiceUtils;

namespace ActiLink.Organizers.BusinessClients.Service
{
    public interface IBusinessClientService
    {
        public Task<GenericServiceResult<BusinessClient>> CreateBusinessClientAsync(string username, string email, string password, string taxId);
        public Task<GenericServiceResult<(string AccessToken, string RefreshToken)>> LoginAsync(string email, string password);

        public Task<IEnumerable<BusinessClient>> GetBusinessClientsAsync();
        public Task<BusinessClient?> GetBusinessClientByIdAsync(string id);
        public Task<ServiceResult> DeleteBusinessClientAsync(BusinessClient businessClient);
        public Task<GenericServiceResult<BusinessClient>> UpdateBusinessClientAsync(string id, UpdateBusinessClientObject updateBusinessClientObject);
	}
}
