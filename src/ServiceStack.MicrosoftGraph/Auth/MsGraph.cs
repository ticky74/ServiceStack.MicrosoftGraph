using System;
using System.Collections.Specialized;

namespace ServiceStack.Azure.Auth
{
    internal class MsGraph
    {
        #region Constants and Variables

        public const string ProviderName = "ms-graph";

        public const string GraphUrl = "https://graph.microsoft.com";

        public const string AuthorizationUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";

        public const string ConsentUrl =
            "https://login.microsoftonline.com/{tenant name}/adminconsent?client_id={application id}&state={some state data}&redirect_uri={redirect uri}";

        public const string TokenUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/token";

        public const string Realm = "https://login.microsoftonline.com/";

        public const string MeUrl = "https://graph.microsoft.com/v1.0/me";
        public const string DirectoryUsersUrl = "https://graph.microsoft.com/v1.0/users/";

        #endregion

        #region Public/Internal

        public static string GetRefreshTokenUrl(string redirectUri, string clientSecret, string refreshToken)
        {
            return $"{TokenUrl}?grant_type=refresh_token&redirect_uri={redirectUri}&client_secret={clientSecret}&refresh_token={refreshToken}&resource=&resource=https%3A%2F%2Fgraph.microsoft.com%2F";
        }
        public static string GetConsentUrl(string tenantId, string applicationId, string state, string redirectUri)
        {
            return
                $"https://login.microsoftonline.com/{tenantId}/adminconsent?client_id={applicationId}&state={state}&redirect_uri={redirectUri}";
        }

        // Implementation taken from @jfoshee Servicestack.Authentication.Aad
        // https://github.com/jfoshee/ServiceStack.Authentication.Aad/blob/master/ServiceStack.Authentication.Aad/AadAuthProvider.cs
        public static bool RespondedWithError(NameValueCollection info)
        {
            return !(info["error"] ?? info["error_uri"] ?? info["error_description"]).IsNullOrEmpty();
        }

        #endregion

        #region Inner Types

        #region ActiveDirectory

        public class ActiveDirectory
        {
            #region Constants and Variables

            public const string MemberGroupsUrl = "https://graph.microsoft.com/v1.0/me/getMemberGroups";

            #endregion
        }

        #endregion

        #endregion
    }

    public class AzureServiceException : Exception
    {
        #region Constructors

        public AzureServiceException(string attemptedUrl, NameValueCollection errorData)
            : base($"Azure graph request failed: {attemptedUrl}")
        {
            ErrorData = errorData;
        }

        #endregion

        #region Properties and Indexers

        public NameValueCollection ErrorData { get; }

        #endregion
    }
}