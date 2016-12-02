using System;
using System.Collections.Generic;
using System.Linq;
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
                var q = db.From<DirectoryUpn>()
                    .Where(x => x.Suffix == loweredDomain);
                return db.Exists<DirectoryUpn>(q);
            }
        }

        public ApplicationRegistration GetApplicationByDirectoryName(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return null;

            var loweredDomain = domain.ToLower();
            using (var db = _connectionFactory.OpenDbConnection())
            {
                var q = db.From<ApplicationRegistration>()
                    .Join<ApplicationRegistration, DirectoryUpn>(
                        (registration, upn) => registration.Id == upn.ApplicationRegistrationId)
                    .Where<DirectoryUpn>(x => x.Suffix == loweredDomain);
                return db.LoadSelect(q).FirstOrDefault();
            }
        }

        public ApplicationRegistration GetApplicationById(string applicationId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                return null;

            using (var db = _connectionFactory.OpenDbConnection())
            {
                return db.LoadSelect<ApplicationRegistration>(d => d.ClientId == applicationId)
                    .FirstOrDefault();
            }
        }

        public ApplicationRegistration RegisterApplication(ApplicationRegistration registration)
        {
            if (registration == null)
                throw new ArgumentException($"Cannot register null or empty {nameof(ApplicationRegistration)}.");

            if (string.IsNullOrWhiteSpace(registration.ClientId))
                throw new ArgumentException("Parameter cannot be empty.", nameof(registration.ClientId));

            if (string.IsNullOrWhiteSpace(registration.ClientSecret))
                throw new ArgumentException("Parameter cannot be empty.", nameof(registration.ClientSecret));

            if (registration.Upns?.Any() == false)
            {
                throw new ArgumentException("At least one upn must be specified to register an application.");
            }

            var duplicates = new List<string>();
            registration.Upns.Each(upn =>
            {
                upn.Suffix = upn.Suffix.ToLower();
                upn.DateCreatedUtc = registration.DateCreatedUtc;
                duplicates.Add(upn.Suffix);
            });

            using (var db = _connectionFactory.OpenDbConnection())
            {
                var existing = db.Select<DirectoryUpn>(x => duplicates.Contains(x.Suffix));
                if (existing.Any())
                {
                    throw new InvalidOperationException($"Specified suffix(es) already registered: {string.Join(",", existing)}");
                }

                db.Save(registration, true);
                return db.Single<ApplicationRegistration>(d => d.Id == registration.Id);
            }
        }

//        public ApplicationRegistration RegisterApplication(string applicationId, string publicKey, string directoryName,
//            long? refId, string refIdStr)
//        {
//            if (string.IsNullOrWhiteSpace(applicationId))
//                throw new ArgumentException("Parameter cannot be empty.", nameof(applicationId));
//
//            if (string.IsNullOrWhiteSpace(publicKey))
//                throw new ArgumentException("Parameter cannot be empty.", nameof(publicKey));
//
//            if (string.IsNullOrWhiteSpace(directoryName))
//                throw new ArgumentException("Parameter cannot be empty.", nameof(directoryName));
//
//           
//
//            using (var db = _connectionFactory.OpenDbConnection())
//            {
//                var loweredDomain = directoryName.ToLower();
//                if (db.Exists<ApplicationRegistration>(d => d.DirectoryName == loweredDomain))
//                    throw new InvalidOperationException($"Aad domain {directoryName} is already registered");
//                var dir = new ApplicationRegistration
//                {
//                    ClientId = applicationId,
//                    ClientSecret = publicKey,
//                    DirectoryName = directoryName,
//                    RefId = refId,
//                    RefIdStr = refIdStr,
//                    AppTenantId = SequentialGuid.Create()
//                };
//                db.Save(dir, true);
//
//                return db.Single<ApplicationRegistration>(d => d.Id == dir.Id);
//            }
//        }

        public int GrantAdminConsent(string directoryName, string username)
        {
            if (string.IsNullOrWhiteSpace(directoryName))
                throw new ArgumentException("Parameter cannot be empty.", nameof(directoryName));

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Parameter cannot be empty.", nameof(username));

            var loweredDomain = directoryName.ToLower();
            using (var db = _connectionFactory.OpenDbConnection())
            { 
                var q = db.From<ApplicationRegistration>()
                    .Join<ApplicationRegistration, DirectoryUpn>((registration, upn) => registration.Id == upn.ApplicationRegistrationId)
                        .Where<DirectoryUpn>(x => x.Suffix == loweredDomain);

                var ar = db.Single<ApplicationRegistration>(q);
                ar.ConsentGrantedBy = username;
                ar.ConstentDateUtc = DateTimeOffset.UtcNow;
                return db.Update(ar);
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