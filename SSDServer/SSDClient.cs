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
    internal class SSDClient
    {
        public const byte CLIENT_LOGIN_REUQEST = 0;
        public const byte CLIENT_EMERGENCY_REUQEST_INVOKED = 1;
        public const byte CLIENT_DESCRIPTION_PROVIDED = 2;
        public const byte CLIENT_EMERGENCY_REUQEST_ACCEPTED = 3;

        private TcpClient socket = null;
        private NetworkStream networkStream = null;
        private byte[] buffer = null;

        public ClientID ClientID { get; set; }

        public event EventHandler<IRequest> EmergencyRequestMade;
        public event EventHandler<IRequest> EmergencyRequestAccepted;
        public event EventHandler<EndPoint> ConnectionClosed;

        public event EventHandler<Exception> ExceptionCatched;

        public SSDClient(TcpClient socket)
        {
            this.socket = socket;
            socket.ReceiveBufferSize = 4096;
            socket.SendBufferSize = 4096;

            buffer = new byte[socket.ReceiveBufferSize];
            networkStream = socket.GetStream();
            networkStream.BeginRead(buffer, 0, socket.ReceiveBufferSize, OnClientSendMessage, null);
        }

        public void RecieveRequest(IRequest req)
        {
            /// Send request message is structured as followed
            /// 
            /// Range | Utility
            /// ------+----------------------------------------------------------------
            /// 0     | 0 (Code for emergency request)
            /// 1-16  | Requestee Guid
            /// 17-20 | Length of room number
            /// 21-24 | Length of location
            /// 25-n  | room number (as a UTF-8 string)
            /// n-m   | location (as a UTF-8 string)
            IEnumerable<byte> msg = new byte[] { 0 };
            msg = msg.Concat(req.getRequestee().clientID.ToByteArray());
            msg = msg.Concat(BitConverter.GetBytes(req.getRaumnummer().Length));
            msg = msg.Concat(BitConverter.GetBytes(req.getStandort().Length));
            msg = msg.Concat(Encoding.UTF8.GetBytes(req.getRaumnummer()));
            msg = msg.Concat(Encoding.UTF8.GetBytes(req.getStandort()));

            byte[] data = msg.ToArray();
            networkStream.Write(data, 0, data.Length);
        }

        public void SendRequestAccept(IRequest req)
        {
            // This is send immediately after a request was accepted 
            networkStream.Write(new byte[] { 2, 1 }, 0, 2);
        }

        private void OnClientSendMessage(IAsyncResult _result)
        {
            if (socket == null)
                return;
            try
            {
                ParseReceivedData(_result);
            }
            catch (Exception e)
            {
                IPEndPoint remoteIpEndPoint = socket.Client.RemoteEndPoint as IPEndPoint;
                ExceptionCatched?.Invoke(this, new Exception(String.Format("Error while receiving data on IP {0}(Port: {1})! Error message: {2}", remoteIpEndPoint.Address, remoteIpEndPoint.Port, e.Message)));
            }
        }

        private void ParseReceivedData(IAsyncResult _result)
        {
            /// Message structure is as followed:
            /// 
            /// Range | Utility
            /// ------+----------------------------------------------------------------------
            /// 0     | Message identifier
            /// 1 - n | Message specific data (see definition in the corresponding functions)
            int bytesRead = networkStream.EndRead(_result);
            if (bytesRead <= 0)
            {
                ConnectionClosed?.Invoke(this, socket.Client.RemoteEndPoint);
                return;
            }
            switch (buffer[0])
            {
                case CLIENT_LOGIN_REUQEST:
                    LoginRequested();
                    break;
                case CLIENT_EMERGENCY_REUQEST_INVOKED:
                    EmergencyRequestInvoked();
                    break;
                case CLIENT_DESCRIPTION_PROVIDED:
                    EmegencyDescriptionProvided();
                    break;
                case CLIENT_EMERGENCY_REUQEST_ACCEPTED:
                    AccecptEmergencyRequest();
                    break;
            }
            networkStream.BeginRead(buffer, 0, socket.ReceiveBufferSize, OnClientSendMessage, null);
        }

        private void AccecptEmergencyRequest()
        {
            /// Login message structure is as followed
            /// 
            /// Range | Utility
            /// ------+---------------------------------------------------------------------
            /// 1-16  | The guid associated with the client
            /// 17-20 | raumnummer length
            /// 21-24 | standort length
            /// 25-n  | raumnummer (as a UTF-8 string)
            /// n-m   | standort (as a UTF-8 string)
            if (new Guid(buffer.AsSpan(1, 16)).CompareTo(ClientID.clientID) != 0)
            {
                Console.WriteLine("[WARNING] Possible malicious package recieved! Recieved invalid Guid compared to Guid assigned on connection!");
                return;
            } else if (((byte)ClientID.clientType & (byte)ClientType.Sanitaeter) == 0)
            {
                Console.WriteLine("[WARNING] Possible malicious package recieved! Emergency request accepted without permission to do so!");
                return;
            }

            int roomLength = BitConverter.ToInt32(buffer, 17);
            int locationLength = BitConverter.ToInt32(buffer, 21);
            string rn = Encoding.UTF8.GetString(buffer.AsSpan(25, roomLength));
            string so = Encoding.UTF8.GetString(buffer.AsSpan(25 + roomLength, locationLength));
            EmergencyRequestAccepted?.Invoke(this,
                SSDServer.Instance.requests.FirstOrDefault(req => req.Value.getRaumnummer() == rn && req.Value.getStandort() == so).Value
            );
        }

        private void LoginRequested()
        {
            /// Login message structure is as followed
            /// 
            /// Range | Utility
            /// ------+---------------------------------------------------------------------
            /// 1     | Role identifier (see ClientID for possible roles)

            /*
             * Here would be the part where the login data and password would be compared to a Db, however for this first test I'll just parse the role
             */

            if (buffer[1] > (byte)ClientType.Admin)
                throw new InvalidDataException(String.Format("Received client role doesn't match any available ones! (Requested role ID: {0})", buffer[1]));
            ClientID = ClientID.NewClientID((ClientType)buffer[1]);

            networkStream.Write(ClientID.clientID.ToByteArray(), 0, 16); // Send the client guid back as confirmation
        }

        private void EmergencyRequestInvoked()
        {
            /// Emergency message structure is as followed
            /// 
            /// Range | Utility
            /// ------+---------------------------------------------------------------------
            /// 1-16  | The guid associated with the client
            /// 17-20 | Length of the room number
            /// 21-24 | Length of the location
            /// 25-n  | room number (as a UTF-8 string)
            /// n-m   | location (as a UTF-8 string)
            if (new Guid(buffer.AsSpan(1, 16)).CompareTo(ClientID.clientID) != 0)
            {
                networkStream.Write(new byte[] { 1, 0 }, 0, 2); // Send 0 as failure
                throw new InvalidDataException("[WARNING] Possible malicious package recieved! Recieved invalid Guid compared to Guid assigned on connection!");
            }
            else if (((byte)ClientID.clientType & (byte)ClientType.Lehrer) == 0)
            {
                networkStream.Write(new byte[] { 1, 0 }, 0, 2);
                throw new InvalidDataException("[WARNING] Possible malicious package recieved! Emergency request made without permission to do so!");
            }

            int roomLength = BitConverter.ToInt32(buffer, 17);
            int locationLength = BitConverter.ToInt32(buffer, 21);
            EmergencyRequestMade?.Invoke(this, 
                new Request ( 
                    ClientID,
                    Encoding.UTF8.GetString(buffer.AsSpan(25, roomLength)), 
                    Encoding.UTF8.GetString(buffer.AsSpan(25 + roomLength, locationLength))
                )
            );

            networkStream.Write(new byte[] { 1, 1 }, 0, 2); // Send 1 to indicate a sucessfull request
        }

        private void EmegencyDescriptionProvided()
        {
            /// Description message structure is as followed
            /// 
            /// Range | Utility
            /// ------+---------------------------------------------------------------------
            /// 1-16  | The guid associated with the client
            /// 17-20 | Length of the description
            /// 21-n  | description (as a UTF-8 string)
            if (new Guid(buffer.AsSpan(1, 16)).CompareTo(ClientID.clientID) != 0)
            {
                Console.WriteLine("[WARNING] Possible malicious package recieved! Recieved invalid Guid compared to Guid assigned on connection!");
                return;
            }

            int descriptionLength = BitConverter.ToInt32(buffer, 17);
            IRequest oldReq = SSDServer.Instance.requests[ClientID];
            SSDServer.Instance.requests[ClientID] = new Request(ClientID, oldReq.getRaumnummer(), oldReq.getStandort(), Encoding.UTF8.GetString(buffer.AsSpan(21, descriptionLength)));
        }

        public void ForceConnectionClose()
        {
            socket.Close();
        }
    }
}
