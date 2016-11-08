using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceStack.Azure
{
    public class MicrosoftGraphFeature : IPlugin
    {
        public void Register(IAppHost appHost)
        {
            appHost.RegisterServicesInAssembly(typeof(MicrosoftGraphFeature).GetAssembly());
        }
    }
}
