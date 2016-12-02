using System.Collections.Specialized;

namespace ServiceStack.MicrosoftGraph.ServiceModel.Requests
{
    public class TokenResponse
    {
        #region Properties and Indexers

        public string AccessToken { get; set; }
        public NameValueCollection AuthData { get; set; }
        public string RefreshToken { get; set; }
        public string TokenExpirationSeconds { get; set; }
        public string IdToken { get; set; }

        #endregion
    }
}