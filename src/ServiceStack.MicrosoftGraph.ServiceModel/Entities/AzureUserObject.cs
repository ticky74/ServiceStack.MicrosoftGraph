using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServiceStack.MicrosoftGraph.ServiceModel.Entities
{
    [DataContract]
    public class AzureUserObject
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public List<string> BusinessPhones { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string GivenName { get; set; }
        [DataMember]
        public object JobTitle { get; set; }
        [DataMember]
        public string Mail { get; set; }
        [DataMember]
        public string MobilePhone { get; set; }
        [DataMember]
        public object OfficeLocation { get; set; }
        [DataMember]
        public string PreferredLanguage { get; set; }
        [DataMember]
        public string Surname { get; set; }
        [DataMember]
        public string UserPrincipalName { get; set; }
    }
}