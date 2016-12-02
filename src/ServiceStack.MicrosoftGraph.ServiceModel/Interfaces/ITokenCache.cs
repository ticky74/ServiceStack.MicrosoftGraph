using System.Threading.Tasks;

namespace ServiceStack.MicrosoftGraph.ServiceModel.Interfaces
{
    public interface ITokenCache
    {
        string GetAccessToken(int userAuthId);

        Task<string> GetAccessTokenAsync(int userAuthId);
    }
}