using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack.MicrosoftGraph.ServiceModel.Entities;
using ServiceStack.MicrosoftGraph.ServiceModel.Interfaces;
using ServiceStack.MicrosoftGraph.ServiceModel.Requests;
using ServiceStack.Text;

namespace ServiceStack.Azure.Auth
{
    public class MicrosoftGraphService : IAzureGraphService
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

        public AzureUserObject Me(string authToken)
        {
            var azureReq = MsGraph.MeUrl.GetStringFromUrl(
                requestFilter: req => { req.AddBearerToken(authToken); });
            var respData = JsonSerializer.DeserializeFromString<GraphResponse<AzureUserObject>>(azureReq);
            return respData.Value;

//            var meInfo = JsonObject.Parse(azureReq);
//            var meInfoNvc = meInfo.ToNameValueCollection();
//            var me = new Me
//            {
//                Email = meInfoNvc["mail"],
//                FirstName = meInfoNvc["givenName"],
//                LastName = meInfoNvc["surname"],
//                Language = meInfoNvc["preferredLanguage"],
//                PhoneNumber = meInfoNvc["mobilePhone"]
//            };
//
//            return me;
        }

        public async Task<AzureUserObject> MeAsync(string authToken)
        {
            var azureReq = await MsGraph.MeUrl.GetStringFromUrlAsync(
                requestFilter: req => { req.AddBearerToken(authToken); });
            var respData = JsonSerializer.DeserializeFromString<GraphResponse<AzureUserObject>>(azureReq);
            return respData.Value;
        }

        public AzureUserObject[] Users(string authToken)
        {
            var azureResponse = MsGraph.DirectoryUsersUrl.GetStringFromUrl(
                requestFilter: req => { req.AddBearerToken(authToken); });
            var respData = JsonSerializer.DeserializeFromString<GraphResponse<AzureUserObject[]>>(azureResponse);
            return respData.Value;
        }

        public async Task<AzureUserObject[]> UsersAsync(string authToken)
        {
            var azureResponse = await MsGraph.DirectoryUsersUrl.GetStringFromUrlAsync(
                requestFilter: req => { req.AddBearerToken(authToken); });
            var respData = JsonSerializer.DeserializeFromString<GraphResponse<AzureUserObject[]>>(azureResponse);
            return respData.Value;
        }


        public AuthCodeRequestData RequestConsentCode(AuthCodeRequest codeRequest)
        {
            var state = Guid.NewGuid().ToString("N");
            var reqUrl = MsGraph.GetConsentUrl(codeRequest.Upn, codeRequest.Registration.ClientId,
                state, codeRequest.CallbackUrl.UrlEncode());
            return new AuthCodeRequestData
            {
                AuthCodeRequestUrl = reqUrl,
                State = state
            };
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
                RefreshToken = authInfo["refresh_token"],
                IdToken = authInfo["id_token"],
                TokenExpirationSeconds = authInfo["expires_in"]
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