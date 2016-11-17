using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack.Azure.Auth;
using ServiceStack.Data;
using ServiceStack.MicrosoftGraph.ServiceModel.Entities;
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
            }
        }
    }
}
