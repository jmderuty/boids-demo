using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Users
{
    public class UserManagementConfig
    {
        public UserManagementConfig()
        {
            AuthenticationProviders = new List<IAuthenticationProvider>();
        }

        public List<IAuthenticationProvider> AuthenticationProviders { get; private set; }
        public string  SceneIdRedirect { get;  set; }
    }
}
