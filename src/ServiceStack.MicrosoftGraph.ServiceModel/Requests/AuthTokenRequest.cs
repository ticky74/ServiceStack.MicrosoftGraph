using ServiceStack.MicrosoftGraph.ServiceModel.Entities;

namespace ServiceStack.MicrosoftGraph.ServiceModel.Requests
{
    public class AuthTokenRequest
    {
        #region Properties and Indexers

        public string CallbackUrl { get; set; }
        public ApplicationRegistration Registration { get; set; }
        public string RequestCode { get; set; }
        public string[] Scopes { get; set; }

        #endregion
    }
}