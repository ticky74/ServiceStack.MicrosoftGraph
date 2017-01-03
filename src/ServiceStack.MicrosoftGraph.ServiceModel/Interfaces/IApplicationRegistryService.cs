using System.Collections.Generic;
using ServiceStack.MicrosoftGraph.ServiceModel.Entities;

namespace ServiceStack.MicrosoftGraph.ServiceModel.Interfaces
{
    public interface IApplicationRegistryService
    {
        #region  Abstract

        bool ApplicationIsRegistered(string directoryName);
        ApplicationRegistration GetApplicationByDirectoryName(string domain);
        ApplicationRegistration GetApplicationById(string tenantId);
        void InitSchema();
        ApplicationRegistration RegisterApplication(ApplicationRegistration registration);
        ApplicationRegistration RegisterUpns(ApplicationRegistration registration, IEnumerable<string> upns);
        int GrantAdminConsent(string directoryName, string username);

        #endregion
    }
}