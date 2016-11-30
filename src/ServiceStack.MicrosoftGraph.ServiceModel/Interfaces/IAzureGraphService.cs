using ServiceStack.MicrosoftGraph.ServiceModel.Entities;
using ServiceStack.MicrosoftGraph.ServiceModel.Requests;

namespace ServiceStack.MicrosoftGraph.ServiceModel.Interfaces
{
    public interface IAzureGraphService
    {
        #region  Abstract

        string[] GetMemberGroups(string authToken);
        Me Me(string authToken);
        AuthCodeRequestData RequestAuthCode(AuthCodeRequest codeRequest);
        AuthCodeRequestData RequestConsentCode(AuthCodeRequest codeRequest);
        TokenResponse RequestAuthToken(AuthTokenRequest tokenRequest);

        #endregion
    }
}