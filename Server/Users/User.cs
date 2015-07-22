using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Server.Users
{
    public class User
    {
        public string Id { get; set; }

        public JObject Auth { get; set; }
        public JObject UserData { get; set; } 
    }
}
