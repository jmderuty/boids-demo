using Stormancer.Authentication;

namespace Stormancer
{
    public static class ClientExtension
    {
        public static AuthenticatorService Authenticator(this Client client)
        {
            return client.DependencyResolver.Resolve<AuthenticatorService>();
        }
    }
}
