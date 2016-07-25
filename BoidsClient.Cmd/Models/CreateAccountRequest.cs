using MsgPack.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stormancer.Authentication.Models
{
    public class CreateAccountRequest
    {
        [MessagePackMember(0)]
        public string Login { get; set; }

        [MessagePackMember(1)]
        public string Password { get; set; }

        [MessagePackMember(2)]
        public string Email { get; set; }

        //Json userdata
        [MessagePackMember(3)]
        public string UserData { get; set; }
    }
}
