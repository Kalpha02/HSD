using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SSDServer.Interfaces;

namespace SSDServer
{
    public class SSDServer
    {
        internal Dictionary<ClientID, IRequest> requests = null;
        private List<SSDClient> connectedClients = null;

        private TcpListener socket = null;
        private static SSDServer instance = null;

        public event EventHandler<IRequest> RequestReceived;
        public event EventHandler<IRequest> RequestAccepted;
        public event EventHandler<EndPoint> ConnectionEstablished;
        public event EventHandler<EndPoint> ConnectionClosed;

        public event EventHandler<Exception> ExceptionCatched;

        public static SSDServer Instance
        {
            get
            {
                if(instance == null)
                    instance = new SSDServer();
                return instance;
            }
        }

        private SSDServer()
        {
            requests = new Dictionary<ClientID, IRequest>();
            connectedClients = new List<SSDClient>();
        }

        public void Start(int port = 16320)
        {
            socket = new TcpListener(IPAddress.Any, port);
            socket.Start();
            socket.BeginAcceptTcpClient(new AsyncCallback(OnClientConnected), null);
        }

        public bool Close(bool ignoreRequests = false)
        {
            if (!ignoreRequests && requests.Count > 0)
                return false;

            for (int i = 0; i < connectedClients.Count; ++i)
                connectedClients[i].ForceConnectionClose();
            instance = null;
            return true;
        }

        private static void OnClientConnected(IAsyncResult _result)
        {
            TcpClient client = instance.socket.EndAcceptTcpClient(_result);
            client.NoDelay = false;
            SSDClient managedClient = new SSDClient(client);
            managedClient.EmergencyRequestMade += (o, req) => {
                instance.requests.Add((o as SSDClient).ClientID, req);
                instance.RequestReceived?.Invoke(instance, req);
                try
                {
                    SendEmergencyRequestToRespnders(req);
                }
                catch (Exception e)
                {
                    instance.ExceptionCatched?.Invoke(instance,
                        new Exception(String.Format("Failed to send emergency request to responders! Error: {0}(Type: {1})", e.Message, e.GetType().ToString()))
                    );
                }
            };
            managedClient.ConnectionClosed += (o, ep) => {
                instance.ConnectionClosed?.Invoke(instance, ep);
                instance.connectedClients.Remove(o as SSDClient);
            };
            managedClient.ExceptionCatched += (o, ex) => {
                instance.ExceptionCatched?.Invoke(instance, new Exception(String.Format("Catched exception when handling client {0}!", (o as SSDClient).ClientID.clientID), ex));
            };
            managedClient.EmergencyRequestAccepted += (o, req) => {
                instance.RequestAccepted?.Invoke(instance, req);
            };
            instance.connectedClients.Add(managedClient);
            instance.ConnectionEstablished?.Invoke(instance, client.Client.RemoteEndPoint);
            instance.socket.BeginAcceptTcpClient(new AsyncCallback(OnClientConnected), null);
        }

        private static void SendEmergencyRequestToRespnders(IRequest req)
        {
            for (int i = 0; i < instance.connectedClients.Count; ++i)
                if (((byte)instance.connectedClients[i].ClientID.clientType & (byte)ClientType.Sanitäter) != 0)
                    instance.connectedClients[i].RecieveRequest(req);
        }

    }
}
