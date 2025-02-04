using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SSDAPI;
using SSDServer.Models;

namespace SSDServer.Tests.Implementations
{
    public class Request : IRequest
    {
        private Guid id;
        private TcpClient client;
        private NetworkStream stream;
        byte[] buffer;

        public Guid ID => id;

        public Request(TcpClient client, Guid id)
        {
            this.client = client;
            this.id = id;

            stream = client.GetStream();
            buffer = new byte[client.ReceiveBufferSize];
        }

        private ServerPackage? GetRequestInfo()
        {
            ClientPackage clientPackage = new ClientPackage(ClientPackage.ClientPackageID.RequestedInfo, new RequestInfo(id, "", "", ""), new AccountInfo(0, "", 0));
            stream.Write(clientPackage.ToByteArray());
            while (stream.Read(buffer, 0, 4096) == 0) { }
            ServerPackage serverPackage = new ServerPackage(buffer);
            if (serverPackage.Acknowledge == 0)
                return null;
            return serverPackage;
        }

        public string getDescription()
        {
            ServerPackage? package = GetRequestInfo();
            if (package == null)
                return "";
            return package.RequestInfo.Description;
        }

        public string getLocation()
        {
            ServerPackage? package = GetRequestInfo();
            if (package == null)
                return "";
            return package.RequestInfo.Location;
        }

        public string getRoomnumber()
        {
            ServerPackage? package = GetRequestInfo();
            if (package == null)
                return "";
            return package.RequestInfo.Roomnumber;
        }
    }
}
