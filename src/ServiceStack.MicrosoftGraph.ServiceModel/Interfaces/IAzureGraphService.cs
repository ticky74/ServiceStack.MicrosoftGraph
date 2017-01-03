using System.Threading.Tasks;
using ServiceStack.MicrosoftGraph.ServiceModel.Entities;
using ServiceStack.MicrosoftGraph.ServiceModel.Requests;

namespace ServiceStack.MicrosoftGraph.ServiceModel.Interfaces
{
    public interface IAzureGraphService
    {
        #region  Abstract

        string[] GetMemberGroups(string authToken);
        string Logout(string clientId, string redirectUrl);
        AzureUserObject Me(string authToken);
        Task<AzureUserObject> MeAsync(string authToken);
        AuthCodeRequestData RequestAuthCode(AuthCodeRequest codeRequest);
        AuthCodeRequestData RequestConsentCode(AuthCodeRequest codeRequest);
        TokenResponse RequestAuthToken(AuthTokenRequest tokenRequest);
        AzureUserObject[] Users(string authToken);
        Task<AzureUserObject[]> UsersAsync(string authToken);
        AzureGroupObject GetGroupByName(string authToken, string groupName);
        Task<AzureGroupObject> GetGroupByNameAsync(string authToken, string groupName);
        AzureUserObject[] UsersByGroup(string authToken, string groupName);
        Task<AzureUserObject[]> UsersByGroupAsync(string authToken, string groupName);

        #endregion
    }
}