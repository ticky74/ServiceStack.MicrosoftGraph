using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Caching;
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

        public class DirectoryRegistrationLookup
        {
            public long RegistryId { get; set; }
            public string Upn { get; set; }
        }

        public class ClientIdRegistrationLookup
        {
            public long RegistryId { get; set; }
            public string ClientId { get; set; }
        }

        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ICacheClient _cacheClient;

        #endregion

        #region Constructors

        public OrmLiteMultiTenantApplicationRegistryService(IDbConnectionFactory connectionFactory, ICacheClient cacheClient)
        {
            _connectionFactory = connectionFactory;
            _cacheClient = cacheClient;
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
            var dirLookup = _cacheClient.GetOrCreate(UrnId.Create(typeof(DirectoryRegistrationLookup), loweredDomain),
                () =>
                {
                    using (var db = _connectionFactory.OpenDbConnection())
                    {
                        var q = db.From<ApplicationRegistration>()
                            .Join<ApplicationRegistration, DirectoryUpn>(
                                (registration, upn) => registration.Id == upn.ApplicationRegistrationId)
                            .Where<DirectoryUpn>(x => x.Suffix == loweredDomain)
                            .Select<ApplicationRegistration>(x => x.Id);

                        var id = db.Column<long>(q).FirstOrDefault();
                        return new DirectoryRegistrationLookup
                        {
                            RegistryId = id,
                            Upn = loweredDomain
                        };
                    }
                });

            return ApplicationById(dirLookup.RegistryId);
        }

        private ApplicationRegistration ApplicationById(long id)
        {
            var reg = _cacheClient.GetOrCreate(UrnId.Create(typeof(ApplicationRegistration), id.ToString()),
                () =>
                {
                    using (var db = _connectionFactory.OpenDbConnection())
                    {
                        var q = db.From<ApplicationRegistration>()
                            .Where(x => x.Id == id);
                        return db.LoadSelect(q).FirstOrDefault();
                    }
                });
            return reg;
        }

        public ApplicationRegistration GetApplicationById(string applicationId)
        {
            if (string.IsNullOrWhiteSpace(applicationId))
                return null;

            // ClientIdRegistrationLookup
            var dirLookup = _cacheClient.GetOrCreate(UrnId.Create(typeof(ClientIdRegistrationLookup), applicationId),
                () =>
                {
                    using (var db = _connectionFactory.OpenDbConnection())
                    {
                        var q = db.From<ApplicationRegistration>()
                            .Where(x => x.ClientId == applicationId)
                            .Select<ApplicationRegistration>(x => x.Id);

                        var id = db.Column<long>(q).FirstOrDefault();
                        return new ClientIdRegistrationLookup
                        {
                            RegistryId = id,
                            ClientId = applicationId
                        };
                    }
                });

            return ApplicationById(dirLookup.RegistryId);
        }

        public ApplicationRegistration RegisterUpns(ApplicationRegistration registration, IEnumerable<string> upns)
        {
            if (registration == null)
                throw new ArgumentException($"Cannot update null or empty {nameof(ApplicationRegistration)}.");

            var utcNow = DateTimeOffset.UtcNow;
            var existing = registration.Upns?.Select(x => x.Suffix.ToLower());
            var unique = upns.Where(x => !string.IsNullOrWhiteSpace(x)
                                         && !existing.Contains(x))
                .Select(x => new DirectoryUpn
                {
                    ApplicationRegistrationId = registration.Id,
                    DateCreatedUtc = utcNow,
                    Suffix = x.ToLower()
                });

            using (var db = _connectionFactory.OpenDbConnection())
            {
                db.InsertAll(unique);

                _cacheClient.RemoveAll(existing.Select(x => UrnId.Create(typeof(DirectoryRegistrationLookup), x)));
                _cacheClient.Remove(UrnId.Create(typeof(ApplicationRegistration), registration.Id.ToString()));

                // Return uncached version to avoid possible race returning invalid cached data.
                var q = db.From<ApplicationRegistration>()
                    .Where(x => x.Id == registration.Id);
                return db.LoadSelect(q).FirstOrDefault();
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

            long id;
            using (var db = _connectionFactory.OpenDbConnection())
            {
                var existing = db.Select<DirectoryUpn>(x => duplicates.Contains(x.Suffix));
                if (existing.Any())
                {
                    throw new InvalidOperationException($"Specified suffix(es) already registered: {string.Join(",", existing)}");
                }

                db.Save(registration, true);
                id = registration.Id;
            }

            _cacheClient.RemoveAll(registration.Upns.Where(x => !string.IsNullOrWhiteSpace(x.Suffix))
                .Select(x => x.Suffix.ToLower()));

            return ApplicationById(id);
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

                var result = db.Update(ar);
                _cacheClient.Remove(UrnId.Create(typeof(ClientIdRegistrationLookup), ar.Id.ToString()));
                return result;
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