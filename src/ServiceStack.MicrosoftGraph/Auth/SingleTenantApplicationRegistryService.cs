using System;
using System.Collections.Generic;
using ServiceStack.MicrosoftGraph.ServiceModel.Entities;
using ServiceStack.MicrosoftGraph.ServiceModel.Interfaces;

namespace ServiceStack.Azure.Auth
{
    public class SingleTenantApplicationRegistryService : IApplicationRegistryService
    {
        #region  Abstract

        internal static class ConfigSettings
        {
            #region Constants and Variables

            public const string ClientId = "oauth.{0}.clientId";
            public const string ClientSecret = "oauth.{0}.clientSecret";
            public const string DirectoryName = "oauth.{0}.directoryName";

            #endregion

            #region Public/Internal

            public static string GetClientIdKey() => GetConfigKey(ClientId);
            public static string GetClientSecretKey() => GetConfigKey(ClientSecret);
            public static string GetDirectoryNameKey() => GetConfigKey(DirectoryName);

            #endregion

            #region Private

            private static string GetConfigKey(string keyFormat)
            {
                return keyFormat.Fmt(MsGraph.ProviderName);
            }

            #endregion
        }

        #endregion

        #region Constants and Variables

        private readonly ApplicationRegistration _registration;
        private HashSet<string> _upns;
        #endregion

        #region Constructors

        public SingleTenantApplicationRegistryService(MicrosoftGraphDirectorySettings settings)
        {
            _upns = new HashSet<string>(new [] {settings.DirectoryName.ToLower()});
            _registration = new ApplicationRegistration
            {
                ClientId = settings.ClientId,
                ClientSecret = settings.ClientSecret
            };
        }

        #endregion

        #region Public/Internal

        public bool ApplicationIsRegistered(string directoryName)
        {
            if (string.IsNullOrWhiteSpace(directoryName))
                return false;
            return _upns.Contains(directoryName.ToLower());
        }

        public ApplicationRegistration GetApplicationByDirectoryName(string domain)
        {
            // Actually disregards the domain parameter. All values are specified
            // statically in the configuration
            return _registration;
        }

        public ApplicationRegistration GetApplicationById(string tenantId)
        {
            // Actually disregards the domain parameter. All values are specified
            // statically in the configuration
            return _registration;
        }

        public ApplicationRegistration RegisterApplication(ApplicationRegistration registration)
        {
            throw new NotImplementedException("Cannot override configured application registration");
        }

        public ApplicationRegistration RegisterApplication(string applicationid, string publicKey, string directoryName,
            long? refId,
            string refIdStr)
        {
            throw new NotImplementedException("Cannot override configured application registration");
        }

        public int GrantAdminConsent(string directoryName, string username)
        {
            _registration.ConstentDateUtc = DateTimeOffset.UtcNow;
            _registration.ConsentGrantedBy = username;
            return 1;
        }

        public void InitSchema()
        {
            // Noop
        }

        #endregion
    }
}