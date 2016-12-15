using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack.Azure.Auth;
using ServiceStack.Data;
using ServiceStack.MicrosoftGraph.ServiceModel.Entities;
using ServiceStack.MicrosoftGraph.ServiceModel.Interfaces;
using ServiceStack.OrmLite;

namespace ServiceStack.Azure
{
    public class MicrosoftGraphFeature : IPlugin
    {
        public void Register(IAppHost appHost)
        {
            MicrosoftGraphAuthenticationProvider.RegisterProviderSupportServices(appHost);
            InitSchema(appHost);
        }

        private void InitSchema(IAppHost appHost)
        {
            var connectionFactory = appHost.TryResolve<IDbConnectionFactory>();
            using (var db = connectionFactory.OpenDbConnection())
            {
                db.CreateTableIfNotExists<ApplicationRegistration>();
                db.CreateTableIfNotExists<DirectoryUpn>();
                var tCache = appHost.TryResolve<ITokenCache>();
                if (tCache is OrmLiteTokenCache)
                {
                    db.CreateTableIfNotExists<UserAuthTokenCache>();
                }
            }
        }

        public static void InitSchema(IDbConnection db, bool initTokenCache = false)
        {
            db.CreateTableIfNotExists<ApplicationRegistration>();
            db.CreateTableIfNotExists<DirectoryUpn>();
            db.CreateTableIfNotExists<UserAuthTokenCache>();
            if (initTokenCache)
            {
                db.CreateTableIfNotExists<UserAuthTokenCache>();
            }
        }
    }
}
