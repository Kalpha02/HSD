using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SSDServer;

namespace SSDClient
{
    public class ConnectionHandler
    {
        private IPAddress _address;
        private Client _client;
        public ConnectionHandler(IPAddress address)
        { 
            _address = address;
            _client = new Client();
            _client.connect(address);

        }

        public void login(string username, string password)
        {
            SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(BitConverter.GetBytes(password.GetHashCode()));
        }
    }
}
