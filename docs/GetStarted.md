## Get Started

1. Register your app with [Azure Developer Apps Portal](https://apps.dev.microsoft.com/Landing).
2. Add the AzureGraphFeature to your AppHost configuration.
```
Plugins.Add(new MicrosoftGraphFeature());
```
2. Register ServiceStack.Authentication.Azure auth provider with the  AuthFeature in your AppHost.cs. The
provider has a Scopes property where you can specify all scopes you would like access to. For 
information on available scopes see [Microsoft Graph Scopes](https://msdn.microsoft.com/library/azure/ad/graph/howto/azure-ad-graph-api-permission-scopes).
```
...
app.Plugins.Add(
    new AuthFeature(() => new AuthUserSession(), 
    new IAuthProvider[]
    {
        new MicrosoftGraphAuthenticationProvider
        {
            Scopes =
                new[]
                {
                    "User.Read", "offline_access", "openid", "profile", "Directory.AccessAsUser.All",
                    "Group.Read.All", "Calendars.ReadWrite", "Calendars.ReadWrite.Shared", "Contacts.Read",
                    "Mail.Send", "Notes.Create", "Notes.Read", "Notes.ReadWrite.CreatedByApp"
                }
        }
    }));
...
```
3. Register an AzureRegistryService with your di container
   
```
// This is the code to register a single tenant in your app
container.Register<IApplicationRegistryService>(
    c => SingleTenantApplicationRegistryService(
            new MicrosoftGraphDirectorySettings
            {
                ClientId = "<ApplicationId From App Registration Page>",
                ClientSecret = "<Super secret password generated on App Reg. Page>",
                DirectoryName = "<Azure directory name i.e. contoso.com>"
            }));

                --- or ---

// This is the code to register multiple tenants in your app
container.Register<IApplicationRegistryService>(
    c => new OrmLiteMultiTenantApplicationRegistryService(c.GetService<IDbConnectionFactory>()));

// You can register multiple tenants in code using init callbacks
AfterInitCallbacks.Add(host =>
{
    using (var db = host.TryResolve<IDbConnectionFactory>().OpenDbConnection())
    {
        var r1 = new ApplicationRegistration
        {
            ClientSecret = "secret1",
            ClientId = "client1",
            DirectoryName = "directoryname1.com",
            AppTenantId = new Guid()
        };

        var r2 = new ApplicationRegistration
        {
            ClientSecret = "secret2",
            ClientId = "client2",
            DirectoryName = "directoryname2.com",
            AppTenantId = new Guid()
        };

        db.InsertAll(new [] {r1, r2});
    }
});

```

4. Post authentication requests to the /auth/ms-graph authentication provider and specify the 
email address (which is the account name) in the UserName property.
```
var auth = new Authenticate {
    provider = "ms-graph",
    UserName = "user@directoryname1.com"
};

                --- or ---

var auth = new Authenticate {
    provider = "ms-graph",
    UserName = "user@directoryname1.com"
};

```

That's all!