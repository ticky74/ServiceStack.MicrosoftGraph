using System;
using ServiceStack.Data;
using ServiceStack.MicrosoftGraph.ServiceModel;
using ServiceStack.MicrosoftGraph.ServiceModel.Entities;
using ServiceStack.MicrosoftGraph.ServiceModel.Interfaces;
using ServiceStack.OrmLite;

namespace ServiceStack.Azure.Auth
{
    public class OrmLiteMultiTenantApplicationRegistryService : IApplicationRegistryService
    {
        #region Constants and Variables

        private readonly IDbConnectionFactory _connectionFactory;

        #endregion

        #region Constructors

        public OrmLiteMultiTenantApplicationRegistryService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        #endregion

        #region IApplicationRegistryService Members

        public bool ApplicationIsRegistered(string directoryName)
        {
            using (var db = _connectionFactory.OpenDbConnection())
            {
                var loweredDomain = directoryName.ToLower();
                return db.Exists<ApplicationRegistration>(d => d.DirectoryName == loweredDomain);
            }
        }

        public ApplicationRegistration GetApplicationByDirectoryName(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return null;

            var loweredDomain = domain.ToLower();
            using (var db = _connectionFactory.OpenDbConnection())
            {
                return db.Single<ApplicationRegistration>(d => d.DirectoryName == loweredDomain);
            }
        }

        public ApplicationRegistration GetApplicationById(string applicationId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                return null;

            using (var db = _connectionFactory.OpenDbConnection())
            {
                return db.Single<ApplicationRegistration>(d => d.ClientId == applicationId);
            }
        }

        public ApplicationRegistration RegisterApplication(ApplicationRegistration registration)
        {
            if (registration == null)
                throw new ArgumentException($"Cannot register null or empty {nameof(ApplicationRegistration)}.");

            return RegisterApplication(registration.ClientId, registration.ClientSecret, registration.DirectoryName,
                registration.RefId, registration.RefIdStr);
        }

        public ApplicationRegistration RegisterApplication(string applicationId, string publicKey, string directoryName,
            long? refId, string refIdStr)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                throw new ArgumentException("Parameter cannot be empty.", nameof(applicationId));

            if (string.IsNullOrWhiteSpace(publicKey))
                throw new ArgumentException("Parameter cannot be empty.", nameof(publicKey));

            if (string.IsNullOrWhiteSpace(directoryName))
                throw new ArgumentException("Parameter cannot be empty.", nameof(directoryName));

            using (var db = _connectionFactory.OpenDbConnection())
            {
                var loweredDomain = directoryName.ToLower();
                if (db.Exists<ApplicationRegistration>(d => d.DirectoryName == loweredDomain))
                    throw new InvalidOperationException($"Aad domain {directoryName} is already registered");

                var id = db.Insert(new ApplicationRegistration
                {
                    ClientId = applicationId,
                    ClientSecret = publicKey,
                    DirectoryName = directoryName,
                    RefId = refId,
                    RefIdStr = refIdStr,
                    AppTenantId = SequentialGuid.Create()
                }, true);

                return db.Single<ApplicationRegistration>(d => d.Id == id);
            }
        }

        public int GrantAdminConsent(string directoryName, string username)
        {
            if (string.IsNullOrWhiteSpace(directoryName))
                throw new ArgumentException("Parameter cannot be empty.", nameof(directoryName));

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Parameter cannot be empty.", nameof(username));

            var loweredDomain = directoryName.ToLower();
            using (var db = _connectionFactory.OpenDbConnection())
            {
                return db.Update<ApplicationRegistration>(
                    new {ConstentDateUtc = DateTimeOffset.UtcNow, ConsentGrantedBy = username},
                    d => d.DirectoryName == loweredDomain);
            }
        }

        public void InitSchema()
        {
            using (var db = _connectionFactory.OpenDbConnection())
            {
                db.CreateTableIfNotExists<ApplicationRegistration>();
            }
        }

        #endregion
    }
}