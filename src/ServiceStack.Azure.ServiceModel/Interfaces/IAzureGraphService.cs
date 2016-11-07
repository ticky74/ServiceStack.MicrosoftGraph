using ServiceStack.Azure.ServiceModel.Entities;
using ServiceStack.Azure.ServiceModel.Requests;

namespace ServiceStack.Azure.ServiceModel.Interfaces
{
    public interface IAzureGraphService
    {
        #region  Abstract

        string[] GetMemberGroups(string authToken);
        Me Me(string authToken);
        AuthCodeRequestData RequestAuthCode(AuthCodeRequest codeRequest);
        TokenResponse RequestAuthToken(AuthTokenRequest tokenRequest);

        #endregion
    }
}