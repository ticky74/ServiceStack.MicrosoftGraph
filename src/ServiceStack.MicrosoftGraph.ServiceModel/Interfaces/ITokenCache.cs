using System.Threading.Tasks;
using ServiceStack.MicrosoftGraph.ServiceModel.Entities;

namespace ServiceStack.MicrosoftGraph.ServiceModel.Interfaces
{
    public interface ITokenCache
    {
        string GetAccessToken(string userName);

        Task<string> GetAccessTokenAsync(string userName);

        void ClearTokenCache(string userName);

        Task ClearTokenCacheAsync(string userName);

        void UpdateTokenCache(UserAuthTokenCache cacheItem);

        Task UpdateTokenCacheAsync(UserAuthTokenCache cacheItem);
    }
}