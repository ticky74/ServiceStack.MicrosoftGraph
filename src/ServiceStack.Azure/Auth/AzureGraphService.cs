using System;
using System.Linq;
using ServiceStack.Azure.ServiceModel.Entities;
using ServiceStack.Azure.ServiceModel.Interfaces;
using ServiceStack.Azure.ServiceModel.Requests;
using ServiceStack.Text;

namespace ServiceStack.Azure.Auth
{
    public class AzureGraphService : IAzureGraphService
    {
        #region Public/Internal

        public string[] GetMemberGroups(string authToken)
        {
            var groups =
                MsGraph.ActiveDirectory.MemberGroupsUrl.PostJsonToUrl("{securityEnabledOnly:false}",
                    requestFilter: req =>
                    {
                        req.AddBearerToken(authToken);
                        req.ContentType = "application/json";
                        req.Accept = "application/json";
                    });

            return JsonSerializer.DeserializeFromString<string[]>(groups);
        }

        public Me Me(string authToken)
        {
            var azureReq = MsGraph.MeUrl.GetStringFromUrl(
                requestFilter: req => { req.AddBearerToken(authToken); });

            var meInfo = JsonObject.Parse(azureReq);
            var meInfoNvc = meInfo.ToNameValueCollection();
            var me = new Me
            {
                Email = meInfoNvc["mail"],
                FirstName = meInfoNvc["givenName"],
                LastName = meInfoNvc["surname"],
                Language = meInfoNvc["preferredLanguage"],
                PhoneNumber = meInfoNvc["mobilePhone"]
            };

            return me;
        }

        public AuthCodeRequestData RequestAuthCode(AuthCodeRequest codeRequest)
        {
            var state = Guid.NewGuid().ToString("N");
            var reqUrl =
                $"{MsGraph.AuthorizationUrl}?client_id={codeRequest.Registration.ClientId}&response_type=code&redirect_uri={codeRequest.CallbackUrl.UrlEncode()}&domain_hint={codeRequest.UserName}&scope={BuildScopesFragment(codeRequest.Scopes)}&state={state}";
            return new AuthCodeRequestData
            {
                AuthCodeRequestUrl = reqUrl,
                State = state
            };
        }

        public TokenResponse RequestAuthToken(AuthTokenRequest tokenRequest)
        {
            if (tokenRequest == null)
                throw new ArgumentNullException(nameof(tokenRequest));

            if (tokenRequest.Registration == null)
                throw new ArgumentException("No directory registration specified.", nameof(tokenRequest.Registration));

            if (string.IsNullOrWhiteSpace(tokenRequest.CallbackUrl))
                throw new ArgumentException("No callback url specified.", nameof(tokenRequest.CallbackUrl));

            if (string.IsNullOrWhiteSpace(tokenRequest.RequestCode))
                throw new ArgumentException("No requests code specified", nameof(tokenRequest.RequestCode));

            if (tokenRequest?.Scopes.Any() == false)
                throw new ArgumentException("No scopes provided", nameof(tokenRequest.Scopes));

            var postData =
                $"grant_type=authorization_code&redirect_uri={tokenRequest.CallbackUrl.UrlEncode()}&code={tokenRequest.RequestCode}&client_id={tokenRequest.Registration.ClientId}&client_secret={tokenRequest.Registration.ClientSecret.UrlEncode()}&scope={BuildScopesFragment(tokenRequest.Scopes)}";
            var result = MsGraph.TokenUrl.PostToUrl(postData);

            var authInfo = JsonObject.Parse(result);
            var authInfoNvc = authInfo.ToNameValueCollection();
            if (MsGraph.RespondedWithError(authInfoNvc))
                throw new AzureServiceException(MsGraph.TokenUrl, authInfoNvc);

            return new TokenResponse
            {
                AuthData = authInfoNvc,
                AccessToken = authInfo["access_token"],
                RefreshToken = authInfo["refresh_token"]
            };
        }

        #endregion

        #region Private

        private string BuildScopesFragment(string[] scopes)
        {
            return scopes.Select(
                scope => $"{MsGraph.GraphUrl}/{scope} ").Join(" ").UrlEncode();
        }

        #endregion
    }
}