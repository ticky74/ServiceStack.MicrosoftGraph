using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceStack.MicrosoftGraph.ServiceModel.Entities
{
    public class AzureGroupObject
    {
        public string Id { get; set; }
        public object Classification { get; set; }
        public string CreatedDateTime { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public List<string> GroupTypes { get; set; }
        public string Mail { get; set; }
        public bool MailEnabled { get; set; }
        public string MailNickname { get; set; }
        public object OnPremisesLastSyncDateTime { get; set; }
        public object OnPremisesSecurityIdentifier { get; set; }
        public object OnPremisesSyncEnabled { get; set; }
        public List<string> ProxyAddresses { get; set; }
        public string RenewedDateTime { get; set; }
        public bool SecurityEnabled { get; set; }
        public string Visibility { get; set; }
    }
}
