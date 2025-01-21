using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SSDServer.Database;
using SSDServer.Models;

namespace SSDServer
{
    internal class SSDClient
    {
        public const byte CLIENT_LOGIN_REQUEST = 0;
        public const byte CLIENT_LOGOUT_REQUEST = 1;
        public const byte CLIENT_EMERGENCY_REUQEST_INVOKED = 2;
        public const byte CLIENT_EMERGENCY_REUQEST_ACCEPTED = 3;
        public const byte CLIENT_ACCOUNT_MODIFIED = 4;
        public const byte CLIENT_REQUEST_INFO = 5;
        public const byte CLIENT_DESCRIPTION_PROVIDED = 6;


        private TcpClient socket = null;
        private NetworkStream networkStream = null;
        private byte[] buffer = null;

        public Account Account { get; set; }

        public event EventHandler<Request> EmergencyRequestMade;
        public event EventHandler<Request> EmergencyRequestAccepted;
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

        public void RecieveRequest(Request req)
        {
<<<<<<< HEAD
            byte[] arr = new ServerPackage(ServerPackage.ServerPackageID.RequestReceive, 1, new RequestInfo(req.id, req.raumnummer, req.standort, req.description), new AccountInfo(Account.ID, Account.Username, Account.Permissions)).ToByteArray();
            networkStream.Write(arr, 0, arr.Length);
=======
            /// Send request message is structured as followed
            /// 
            /// Range | Utility
            /// ------+----------------------------------------------------------------
            /// 0     | AcceptModificationID TODO!
            /// 1-16  | Requestee Guid (RequestID)
            /// 17-20 | Length of room number (Raumnummerlänge)
            /// 21-24 | Length of location (Standortlänge)
            /// 25-28 | Length of description (Beschreibunglänge)
            /// 29-n  | room number (as a UTF-8 string)
            /// n-m   | location (as a UTF-8 string)
            /// m-o   | description (as a UTF-8 String)
            IEnumerable<byte> msg = new byte[] { 0 };
            msg = msg.Concat(req.getRequestee().clientID.ToByteArray());
            msg = msg.Concat(BitConverter.GetBytes(req.getRaumnummer().Length));
            msg = msg.Concat(BitConverter.GetBytes(req.getStandort().Length));
            msg = msg.Concat(BitConverter.GetBytes(req.getDescription().Length));
            msg = msg.Concat(Encoding.UTF8.GetBytes(req.getRaumnummer()));
            msg = msg.Concat(Encoding.UTF8.GetBytes(req.getStandort()));
            msg = msg.Concat(Encoding.UTF8.GetBytes(req.getDescription()));
            /// AcceptMOdificationID currently mising

            byte[] data = msg.ToArray();
            networkStream.Write(data, 0, data.Length);
>>>>>>> 8a6043ecb8048e5b0fb844f440fd14358f357b05
        }

        public void SendRequestAccept(Request req)
        {
            byte[] arr = new ServerPackage(ServerPackage.ServerPackageID.RequestAccepted, 1, new RequestInfo(req.id, req.raumnummer, req.standort, req.description), new AccountInfo(Account.ID, Account.Username, Account.Permissions)).ToByteArray();
            networkStream.Write(arr, 0, arr.Length);
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
                case CLIENT_LOGIN_REQUEST:
                    LoginRequested();
                    break;
                case CLIENT_LOGOUT_REQUEST:
                    LogoutRequested();
                    break;
                case CLIENT_EMERGENCY_REUQEST_INVOKED:
                    EmergencyRequestInvoked();
                    break;
                case CLIENT_EMERGENCY_REUQEST_ACCEPTED:
                    AcceptEmergencyRequest();
                    break;
                case CLIENT_ACCOUNT_MODIFIED:
                    AccountModified();
                    break;
                case CLIENT_REQUEST_INFO:
                    EmergencyInfoRequested();
                    break;
                case CLIENT_DESCRIPTION_PROVIDED:
                    EmegencyDescriptionProvided();
                    break;
            }
            networkStream.BeginRead(buffer, 0, socket.ReceiveBufferSize, OnClientSendMessage, null);
        }

        private void EmergencyInfoRequested()
        {
            ClientPackage package = new ClientPackage(buffer);

            byte[] data = new ServerPackage(ServerPackage.ServerPackageID.RequestInfo, 1, package.RequestInfo, new AccountInfo(Account.ID, Account.Username, Account.Permissions)).ToByteArray();
            networkStream.Write(data, 0, data.Length);
        }

        private void AccountModified()
        {
            throw new NotImplementedException();
        }

        private void LogoutRequested()
        {
            throw new NotImplementedException();
        }

        private void AcceptEmergencyRequest()
        {
            ClientPackage package = new ClientPackage(buffer);

            if (((byte)package.AccountInfo.Permissions & (byte)Account.AccountPermissions.Receiver) == 0)
            {
                Console.WriteLine("[WARNING] Possible malicious package recieved! Emergency request accepted without permission to do so!");
                return;
            }

            SendRequestAccept(SSDServer.Instance.requests[package.RequestInfo.ID]);

            EmergencyRequestAccepted?.Invoke(this,
                SSDServer.Instance.requests[package.RequestInfo.ID]
            );
        }

        private void LoginRequested()
        {
            ClientPackage package = new ClientPackage(buffer);
            // Check if an account with this username and password exists
            Account? accInfo = ClientDB.Instance.Accounts.FirstOrDefault(acc => acc.PasswordHash.SequenceEqual(package.AccountInfo.PasswordHash) && acc.Username.Equals(package.AccountInfo.Username));
            if (accInfo == null)
            {
                byte[] err = new ServerPackage(ServerPackage.ServerPackageID.RequestAcknowledged, 0, new RequestInfo(Guid.Empty, "", "", ""), new AccountInfo(0, "", 0)).ToByteArray();
                networkStream.Write(err, 0, err.Length);
                this.ExceptionCatched?.Invoke(this, new Exception(String.Format("Failed login attempt from {0}", socket.Client.RemoteEndPoint)));
                return;
            }
            Account = accInfo;

            byte[] bytes = new ServerPackage(ServerPackage.ServerPackageID.Login, 1, new RequestInfo(Guid.Empty, "", "", ""), new AccountInfo(accInfo.ID, accInfo.Username, accInfo.Permissions)).ToByteArray();
            networkStream.Write(bytes, 0, bytes.Length);
        }

        private void EmergencyRequestInvoked()
        {
            ClientPackage package = new ClientPackage(buffer);

            if (((byte)Account.Permissions & (byte)Account.AccountPermissions.Requester) == 0)
            {
                byte[] err = new ServerPackage(ServerPackage.ServerPackageID.RequestAcknowledged, 0, new RequestInfo(Guid.Empty, "", "", ""), new AccountInfo(0, "", 0)).ToByteArray();
                networkStream.Write(err, 0, err.Length);
                throw new InvalidDataException("[WARNING] Possible malicious package recieved! Emergency request made without permission to do so!");
            }

            Guid RequestID = Guid.NewGuid();
            byte[] bytes = new ServerPackage(ServerPackage.ServerPackageID.RequestAcknowledged, 1, new RequestInfo(RequestID, package.RequestInfo.Roomnumber, package.RequestInfo.Location, package.RequestInfo.Description), new AccountInfo(Account.ID, Account.Username, Account.Permissions)).ToByteArray();
            networkStream.Write(bytes, 0, bytes.Length);

            SSDServer.Instance.requests.Add(RequestID, new Request(RequestID, package.RequestInfo.Roomnumber, package.RequestInfo.Location, package.RequestInfo.Description));
            EmergencyRequestMade?.Invoke(this, SSDServer.Instance.requests[RequestID]);
        }

        private void EmegencyDescriptionProvided()
        {
            ClientPackage package = new ClientPackage(buffer);

            if (((byte)Account.Permissions & (byte)Account.AccountPermissions.Requester) == 0)
            {
                byte[] err = new ServerPackage(ServerPackage.ServerPackageID.RequestAcknowledged, 0, new RequestInfo(Guid.Empty, "", "", ""), new AccountInfo(0, "", 0)).ToByteArray();
                networkStream.Write(err, 0, err.Length);
                throw new InvalidDataException("[WARNING] Possible malicious package recieved! Emergency request description provided without permission to do so!");
            }
            if (!SSDServer.Instance.requests.TryGetValue(package.RequestInfo.ID, out Request req))
                throw new Exception("[ERROR] Tried to add description to non existend request!");

            SSDServer.Instance.requests[package.RequestInfo.ID] = new Request(package.RequestInfo.ID, req.raumnummer, req.standort, package.RequestInfo.Description);
        }

        public void ForceConnectionClose()
        {
            socket.Close();
        }
    }
}
