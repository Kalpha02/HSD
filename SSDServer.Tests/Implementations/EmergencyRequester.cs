using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SSDServer.Interfaces;
using SSDServer.Tests.Extensions;

namespace SSDServer.Tests.Implementations
{
    internal class EmergencyRequester : IEmergencyRequest
    {
        TcpClient tcpClient;
        byte[] data = null;
        IRequest recievedrequest = null;
        bool requestProcessed = false;

        private const byte EMERGENCY_REQUEST_RECEIVED = 0;
        private const byte EMERGENCY_REQUEST_PROCESSED = 1;
        private const byte EMERGENCY_REQUEST_ACCEPTED = 2;

        internal EmergencyRequester(TcpClient client)
        {
            tcpClient = client;
            data = new byte[client.ReceiveBufferSize];
            client.GetStream().BeginRead(data, 0, client.ReceiveBufferSize, new AsyncCallback(ServerMessageRecieved), null);
        }

        private void ServerMessageRecieved(IAsyncResult ar)
        {
            if (tcpClient.Client.IsDisposed())
                return;
            int bytesRead = tcpClient.GetStream().EndRead(ar);
            if (bytesRead <= 0) // Server side close
                return;
            switch(data[0])
            {
                case EMERGENCY_REQUEST_RECEIVED:
                    ParseEmergencyRequest();
                    break;
                case EMERGENCY_REQUEST_PROCESSED:
                    requestProcessed = true;
                    break;
                case EMERGENCY_REQUEST_ACCEPTED:
                    requestProcessed = true;
                    break;

            }
            if (tcpClient.Client.IsDisposed())
                return;
            tcpClient.GetStream().BeginRead(data, 0, tcpClient.ReceiveBufferSize, new AsyncCallback(ServerMessageRecieved), null);
        }

        private void ParseEmergencyRequest()
        {
            int raumnummerLength = BitConverter.ToInt32(data, 17);
            int standortLength = BitConverter.ToInt32(data, 21);
            recievedrequest = new Request(new ClientID() { clientID = new Guid(data.AsSpan(1, 16)) }, Encoding.UTF8.GetString(data.AsSpan(25, raumnummerLength)), Encoding.UTF8.GetString(data.AsSpan(25 + raumnummerLength, standortLength)));
        }

        public bool CheckRequests(ClientID id, out IRequest request)
        {
            request = recievedrequest;
            if(request == null)
                return false;
            recievedrequest = null;
            return true;
        }

        public Task<bool>? MakeRequest(ClientID id, string raumnummer, string standort)
        {
            byte[] buffer = new byte[] { 1 };
            buffer = buffer.Concat(id.clientID.ToByteArray()).ToArray();
            buffer = buffer.Concat(BitConverter.GetBytes(raumnummer.Length)).ToArray();
            buffer = buffer.Concat(BitConverter.GetBytes(standort.Length)).ToArray();
            buffer = buffer.Concat(Encoding.UTF8.GetBytes(raumnummer)).ToArray();
            buffer = buffer.Concat(Encoding.UTF8.GetBytes(standort)).ToArray();

            requestProcessed = false;
            if (tcpClient.Client.IsDisposed())
                return null;
            tcpClient.GetStream().Write(buffer, 0, buffer.Length);

            while (!requestProcessed)
                Thread.Sleep(100);

            requestProcessed = false;
            if (data[1] == 0) // Request wasn't recognized
                return null;

            return Task.Run(() =>
            {
                while (!requestProcessed)
                    Thread.Sleep(100);

                if (data[1] == 0) // Request wasn't accepted
                    return false;
                return true;
            });
        }

        public bool SendDescription(ClientID id, string description)
        {
            byte[] buffer  = new byte[] { 2 };
            buffer = buffer.Concat(id.clientID.ToByteArray()).ToArray();
            buffer = buffer.Concat(BitConverter.GetBytes(description.Length)).ToArray();
            buffer = buffer.Concat(Encoding.UTF8.GetBytes(description)).ToArray();

            if (tcpClient.Client.IsDisposed())
                return false;
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

            if (tcpClient.Client.IsDisposed())
                return;
            tcpClient.GetStream().Write(buffer, 0, buffer.Length);
        }
    }
}
