using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SSDAPI;
using SSDServer.Models;
using SSDServer.Tests.Extensions;

namespace SSDServer.Tests.Implementations
{
    internal class EmergencyRequester : IEmergencyRequest
    {
        TcpClient tcpClient;
        NetworkStream stream;
        byte[] buffer;

        public EmergencyRequester(TcpClient client)
        {
            tcpClient = client;
            tcpClient.ReceiveBufferSize = 4096;
            tcpClient.SendBufferSize = 4096;

            stream = client.GetStream();
            buffer = new byte[tcpClient.ReceiveBufferSize];
        }

        public void AcceptRequest(IRequest request)
        {
            RequestInfo req = new RequestInfo(request.ID, request.getRoomnumber(), request.getLocation(), request.getDescription());
            ClientPackage package = new ClientPackage(ClientPackage.ClientPackageID.RequestAccepted, req, new AccountInfo(0, "", 0));
            byte[] data = package.ToByteArray();

            stream.Write(data, 0, data.Length);
        }

        public bool CheckRequests(out IRequest request)
        {
            request = null;
            if (stream.Read(buffer, 0, 4096) == 0)
                return false;

            ServerPackage package = new ServerPackage(buffer);
            if(package.Acknowledge == 0)
                return false;

            request = new Request(tcpClient, package.RequestInfo.ID);
            return true;
        }

        public Task<bool>? MakeRequest(string raumnummer, string standort)
        {
            ClientPackage package = new ClientPackage(ClientPackage.ClientPackageID.Request, new RequestInfo(Guid.Empty, raumnummer, standort, ""), new AccountInfo(0, "", 0));
            byte[] data = package.ToByteArray();
            stream.Write(data, 0, data.Length);

            CancellationTokenSource source = new CancellationTokenSource();
            bool success = false;
            Task<bool> t = Task.Run(() =>
            {
                while (source.IsCancellationRequested)
                    Thread.Sleep(100);
                return success;
            });

            stream.BeginRead(buffer, 0, buffer.Length, (ires) =>
            {
                stream.EndRead(ires);
                ServerPackage serverPackage = new ServerPackage(buffer);
                if (serverPackage.Acknowledge != 0)
                    success = true;

                source.Cancel();
            }, null);

            return t;
        }

        public bool SendDescription(string description)
        {
        }
    }
}
