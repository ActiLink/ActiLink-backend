﻿using ActiLink.Shared.ServiceUtils;

namespace ActiLink.Organizers.BusinessClients.Service
{
    public interface IBusinessClientService
    {
        public Task<GenericServiceResult<BusinessClient>> CreateBusinessClientAsync(string username, string email, string password, string taxId);

        public Task<IEnumerable<BusinessClient>> GetBusinessClientsAsync();
        public Task<BusinessClient?> GetBusinessClientByIdAsync(string id);
        public Task<ServiceResult> DeleteBusinessClientAsync(BusinessClient businessClient);
    }
}
