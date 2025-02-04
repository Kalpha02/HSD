using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SSDAPI.Interfaces;
using SSDAPI.Models;

namespace SSDServer.Tests.Implementations
{
    public class Request : IRequest
    {
        private Guid id;
        private EmergencyRequester requester;
        private NetworkStream stream;

        public Guid ID => id;

        public Request(NetworkStream stream, EmergencyRequester requester, Guid id)
        {
            this.id = id;
            this.requester = requester;
            this.stream = stream;
        }

        private ServerPackage? GetRequestInfo()
        {
            ClientPackage clientPackage = new ClientPackage(ClientPackage.ClientPackageType.RequestInfo, new AccountInfo(0, "", new byte[32], 0), new RequestInfo(id, "", "", ""));
            stream.Write(clientPackage.ToByteArray());
            ServerPackage serverPackage = null;
            requester.ReceivedPackage += (o, p) =>
            {
                if (p.PackageType == ServerPackage.ServerPackageID.RequestInfo)
                    serverPackage = p;
            };
            while (serverPackage == null) { }
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
