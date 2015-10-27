using Stormancer.Authentication;
using System;

namespace Stormancer
{
    public static class ClientExtension
    {
        public static AuthenticatorService Authenticator(this Client client)
        {
            return client.DependencyResolver.GetComponent<AuthenticatorService>();
        }
    }
}
