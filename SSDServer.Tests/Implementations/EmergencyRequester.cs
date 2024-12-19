using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SSDServer.Interfaces;

namespace SSDServer.Tests.Implementations
{
    internal class EmergencyRequester : IEmergencyRequest
    {
        TcpClient tcpClient;
        byte[] data = null;
        IRequest recievedrequest = null;

        private const byte EMERGENCY_REQUEST_RECEIVED = 0;

        internal EmergencyRequester(TcpClient client)
        {
            tcpClient = client;
            data = new byte[client.ReceiveBufferSize];
            client.GetStream().BeginRead(data, 0, client.ReceiveBufferSize, new AsyncCallback(ServerMessageRecieved), null);
        }

        private void ServerMessageRecieved(IAsyncResult ar)
        {
            int bytesRead = tcpClient.GetStream().EndRead(ar);
            if (bytesRead <= 0)
                return;
            switch(data[0])
            {
                case EMERGENCY_REQUEST_RECEIVED:
                    ParseEmergencyRequest();
                    break;
            }
            tcpClient.GetStream().BeginRead(data, 0, tcpClient.ReceiveBufferSize, new AsyncCallback(ServerMessageRecieved), null);
        }

        private void ParseEmergencyRequest()
        {
            int raumnummerLength = BitConverter.ToInt32(data, 1);
            int standortLength = BitConverter.ToInt32(data, 5);
            recievedrequest = new Request(Encoding.UTF8.GetString(data.AsSpan(9, raumnummerLength)), Encoding.UTF8.GetString(data.AsSpan(9 + raumnummerLength, standortLength)));
        }

        public bool CheckRequests(ClientID id, out IRequest request)
        {
            request = recievedrequest;
            if(request == null)
                return false;
            recievedrequest = null;
            return true;
        }

        public Task<bool> MakeRequest(ClientID id, string raumnummer, string standort)
        {
            byte[] buffer = new byte[] { 1 };
            buffer = buffer.Concat(id.clientID.ToByteArray()).ToArray();
            buffer = buffer.Concat(BitConverter.GetBytes(raumnummer.Length)).ToArray();
            buffer = buffer.Concat(BitConverter.GetBytes(standort.Length)).ToArray();
            buffer = buffer.Concat(Encoding.UTF8.GetBytes(raumnummer)).ToArray();
            buffer = buffer.Concat(Encoding.UTF8.GetBytes(standort)).ToArray();

            tcpClient.GetStream().Write(buffer, 0, buffer.Length);
            IAsyncResult res = tcpClient.GetStream().BeginRead(buffer, 0, 1, (res) =>
            {
                tcpClient.GetStream().EndRead(res);
            }, null);

            return Task.Run<bool>(() =>
            {
                while(!res.IsCompleted)
                    Thread.Sleep(100);

                if (buffer[0] == 0)
                    return false;

                bool accepted = false;
                SSDServer.Instance.RequestAccepted += (o, req) => { // !!!WICHTIG!!! Das funktioniert nur während dem testen nicht in einem real world szenario!!!!!!!!!!!
                    if(req.getRaumnummer() == raumnummer && req.getStandort() == standort)
                        accepted = true;
                };

                while (!accepted)
                    Thread.Sleep(100);
                return true;
            });
        }

        public bool SendDescription(ClientID id, string description)
        {
            byte[] buffer  = new byte[] { 2 };
            buffer = buffer.Concat(id.clientID.ToByteArray()).ToArray();
            buffer = buffer.Concat(BitConverter.GetBytes(description.Length)).ToArray();
            buffer = buffer.Concat(Encoding.UTF8.GetBytes(description)).ToArray();

            tcpClient.GetStream().Write(buffer, 0, buffer.Length);
            return true;
        }

        public void AcceptRequest(ClientID id, IRequest request)
        {
            byte[] buffer = new byte[] { 3 };
            buffer = buffer.Concat(id.clientID.ToByteArray()).ToArray();
            buffer = buffer.Concat(BitConverter.GetBytes(request.getRaumnummer().Length)).ToArray();
            buffer = buffer.Concat(BitConverter.GetBytes(request.getStandort().Length)).ToArray();
            buffer = buffer.Concat(Encoding.UTF8.GetBytes(request.getRaumnummer())).ToArray();
            buffer = buffer.Concat(Encoding.UTF8.GetBytes(request.getStandort())).ToArray();

            tcpClient.GetStream().Write(buffer, 0, buffer.Length);
        }
    }
}
