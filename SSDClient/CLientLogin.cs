using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SSDServer
{
    internal class CLientLogin
    {
        public int id { get; set; }
        public string username { get; set; }
        public SecureString password { get; set; }

        public CLientLogin(int id, string username, SecureString password) 
        {
            this.id = id;
            this.username = username;
            this.password = password;
            
        }

        public void Login(string ipAdress)
        {  
            NetworkStream connection = null;
            /// send credentials to server
        }

    }
}
