using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SSDAPI.Models;
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

            byte[] Ersetzen1 = Array.Empty<byte>();
            int Ersetzen2 = 0;

            ClientPackage clientPackage = new ClientPackage(ClientPackage.ClientPackageType.Login, new AccountInfo(0, username, Ersetzen1, Ersetzen2), new RequestInfo(Guid.Empty, "", "", ""));
            sendToServer(clientPackage.ToByteArray());
        }

        public void sendToServer(byte[] payload) 
        { 
            _client.stream.Write(payload, 0, payload.Length);
        }
    }
}
