using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SSDAPI.Interfaces;
using SSDServer.Database;

namespace SSDServer
{
    public class SSDServer
    {
        internal Dictionary<Guid, Request> requests = null;
        private List<SSDClient> connectedClients = null;

        private TcpListener socket = null;
        private static SSDServer instance = null;
        private IAsyncResult clientAcceptResult = null;

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
            requests = new Dictionary<Guid, Request>();
            connectedClients = new List<SSDClient>();
        }

        public void Start(int port = 16320)
        {
            socket = new TcpListener(IPAddress.Any, port);
            socket.Start();
            clientAcceptResult = socket.BeginAcceptTcpClient(new AsyncCallback(OnClientConnected), null);
        }

        public bool Close(bool ignoreRequests = false)
        {
            if (!ignoreRequests && requests.Count > 0)
                return false;

            instance.socket.Stop();
            instance = null;
            return true;
        }

        private static void OnClientConnected(IAsyncResult _result)
        {
            if (instance == null)
                return;
            TcpClient client = instance.socket.EndAcceptTcpClient(_result);
            instance.ConnectionEstablished?.Invoke(instance, client.Client.RemoteEndPoint);
            client.NoDelay = false;
            SSDClient managedClient = new SSDClient(client);
            Instance.connectedClients.Add(managedClient);
            managedClient.EmergencyRequestMade += (o, req) => {
                if (instance == null)
                    return;
                instance.RequestReceived?.Invoke(instance, req);
                SendEmergencyRequestToRespnders(req);
            };
            managedClient.ConnectionClosed += (o, ep) => {
                if (instance == null)
                    return;
                instance.ConnectionClosed?.Invoke(instance, ep);
                instance.connectedClients.Remove(o as SSDClient);
            };
            managedClient.ExceptionCatched += (o, ex) => {
                if (instance == null)
                    return;
                instance.ExceptionCatched?.Invoke(instance, new Exception(String.Format("Catched exception when handling client {0}!", (o as SSDClient).Account.Username), ex));
            };
            managedClient.EmergencyRequestAccepted += (o, req) => {
                instance.RequestAccepted?.Invoke(instance, req);
            };

            instance.clientAcceptResult = instance.socket.BeginAcceptTcpClient(new AsyncCallback(OnClientConnected), null);
        }

        private static void SendEmergencyRequestToRespnders(Request req)
        {
            for (int i = 0; i < instance.connectedClients.Count; ++i)
            {
                if ((instance.connectedClients[i].Account.Permissions & (int)Account.AccountPermissions.Superuser) != 0 || (instance.connectedClients[i].Account.Permissions & (int)Account.AccountPermissions.Receiver) != 0)
                {
                    try
                    {
                        instance.connectedClients[i].RecieveRequest(req);
                    }catch(Exception e) 
                    {
                        instance.ExceptionCatched?.Invoke(instance, e);
                    }
                }
            }
        }

    }
}
