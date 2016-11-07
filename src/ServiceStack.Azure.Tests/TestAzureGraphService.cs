using System.Collections.Specialized;
using ServiceStack.Azure.Auth;
using ServiceStack.Azure.ServiceModel.Entities;
using ServiceStack.Azure.ServiceModel.Interfaces;
using ServiceStack.Azure.ServiceModel.Requests;

namespace ServiceStack.Azure.Tests
{
    public class TestAzureGraphService : IAzureGraphService
    {
        #region Constants and Variables

        public readonly NameValueCollection AuthInfo = new NameValueCollection
        {
            {"access_token", TokenHelper.AccessToken},
            {"id_token", TokenHelper.IdToken}
        };

        #endregion

        #region IAzureGraphService Members

        public AuthCodeRequestData RequestAuthCode(AuthCodeRequest codeRequest)
        {
            // RequestAuthCode
            return new AzureGraphService().RequestAuthCode(codeRequest);
        }

        public Me Me(string authToken)
        {
            return new Me
            {
                Email = "some.user@foodomain.com",
                FirstName = "Some",
                Language = "en",
                LastName = "User",
                PhoneNumber = "15555551212"
            };
        }

        public string[] GetMemberGroups(string authToken)
        {
            return new[] {"2dc78bbb-96a9-4058-b05d-be6e2bcf0ace"};
        }

        public TokenResponse RequestAuthToken(AuthTokenRequest tokenRequest)
        {
            var idToken = TokenHelper.GetIdToken();

            var nvc = new NameValueCollection
            {
                {"access_token", TokenHelper.AccessToken},
                {"id_token", idToken}
            };

            TokenHelper.HydrateAuthInfo(idToken, nvc);
            return new TokenResponse
            {
                AccessToken = TokenHelper.AccessToken,
                RefreshToken = TokenHelper.RefreshToken,
                AuthData = nvc
            };
        }

        #endregion
    }
}