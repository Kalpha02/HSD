using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SSDAPI.Interfaces;
using SSDAPI.Models;
using SSDServer.Tests.Extensions;

namespace SSDServer.Tests.Implementations
{
    public class EmergencyRequester : IEmergencyRequest
    {
        TcpClient tcpClient;
        NetworkStream stream;
        byte[] buffer;
        public List<ServerPackage> queuedPackages = new List<ServerPackage>();
        Guid lastRequestID = Guid.Empty;

        public event EventHandler<ServerPackage> ReceivedPackage;

        public EmergencyRequester(TcpClient client)
        {
            tcpClient = client;
            tcpClient.ReceiveBufferSize = 4096;
            tcpClient.SendBufferSize = 4096;

            stream = client.GetStream();
            buffer = new byte[tcpClient.ReceiveBufferSize];
            stream.BeginRead(buffer, 0, buffer.Length, OnRequesterDataReceived, null);
        }

        private void OnRequesterDataReceived(IAsyncResult res)
        {
            int byteCount = stream.EndRead(res);
            queuedPackages.Add(new ServerPackage(buffer));
            ReceivedPackage?.Invoke(this, queuedPackages.Last());
            stream.BeginRead(buffer, 0, buffer.Length, OnRequesterDataReceived, null);
        }

        public void AcceptRequest(IRequest request)
        {
            RequestInfo req = new RequestInfo(request.ID, request.getRoomnumber(), request.getLocation(), request.getDescription());
            ClientPackage package = new ClientPackage(ClientPackage.ClientPackageType.RequestAccepted, new AccountInfo(0, "", new byte[32], 0), req);
            byte[] data = package.ToByteArray();

            stream.Write(data, 0, data.Length);
        }

        public bool CheckRequests(out IRequest request)
        {
            request = null;
            ServerPackage pck = queuedPackages.FirstOrDefault(pck=>pck.PackageType == ServerPackage.ServerPackageID.RequestReceive);
            if (pck == null)
                return false;
            request = new Request(tcpClient.GetStream(), this, pck.RequestInfo.ID);
            queuedPackages.Remove(pck);
            return true;
        }

        public Task<bool>? MakeRequest(string raumnummer, string standort)
        {
            ClientPackage package = new ClientPackage(ClientPackage.ClientPackageType.Request, new AccountInfo(0, "", new byte[32], 0), new RequestInfo(Guid.Empty, raumnummer, standort, ""));
            byte[] data = package.ToByteArray();
            stream.Write(data, 0, data.Length);

            return Task.Run(() =>
            {
                ServerPackage pck = null;
                this.ReceivedPackage += (o, p) =>
                {
                    pck = p;
                };
                while (pck == null) { }
                if (pck.Acknowledge == 0)
                    return false;
                lastRequestID = pck.RequestInfo.ID;
                queuedPackages.Remove(pck);
                return true;
            });
        }

        public bool SendDescription(string description)
        {
            byte[] data = new ClientPackage(ClientPackage.ClientPackageType.RequestDescription, new AccountInfo(0, "", new byte[32], 0), new RequestInfo(lastRequestID, "", "", description)).ToByteArray();
            stream.Write(data, 0, data.Length);
            return true;
        }
    }
}
