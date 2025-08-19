using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SSDServer.Database;
using SSDAPI.Models;

namespace SSDServer
{
    internal class SSDClient
    {
        public enum PackageType : byte
        {
            // The first byte stands for the type of the data package
            CLIENT_LOGIN_REQUEST = 0,
            CLIENT_LOGOUT_REQUEST = 1,
            CLIENT_EMERGENCY_REUQEST_INVOKED = 2,
            CLIENT_EMERGENCY_REUQEST_ACCEPTED = 3,
            CLIENT_ACCOUNT_MODIFIED = 4,
            CLIENT_REQUEST_INFO = 5,
            CLIENT_DESCRIPTION_PROVIDED = 6,
        }


        private TcpClient socket { get; init; }
        private NetworkStream networkStream { get; init; }
        private byte[] buffer { get; init; }

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
            byte[] arr = new ServerPackage(ServerPackage.ServerPackageType.RequestReceive, 1, new RequestInfo(req.ID, req.raumnummer, req.standort, req.description), new AccountInfo(Account.ID, Account.Username, new byte[32], Account.Permissions)).ToByteArray();
            networkStream.Write(arr, 0, arr.Length);
        }

        public void SendRequestAccept(Request req)
        {
            byte[] arr = new ServerPackage(ServerPackage.ServerPackageType.RequestAccepted, 1, new RequestInfo(req.ID, req.raumnummer, req.standort, req.description), new AccountInfo(Account.ID, Account.Username, new byte[32], Account.Permissions)).ToByteArray();
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
            switch ((PackageType)buffer[0])
            {
                case PackageType.CLIENT_LOGIN_REQUEST:
                    LoginRequested();
                    break;
                case PackageType.CLIENT_LOGOUT_REQUEST:
                    LogoutRequested();
                    break;
                case PackageType.CLIENT_EMERGENCY_REUQEST_INVOKED:
                    EmergencyRequestInvoked();
                    break;
                case PackageType.CLIENT_EMERGENCY_REUQEST_ACCEPTED:
                    AcceptEmergencyRequest();
                    break;
                case PackageType.CLIENT_ACCOUNT_MODIFIED:
                    AccountModified();
                    break;
                case PackageType.CLIENT_REQUEST_INFO:
                    EmergencyInfoRequested();
                    break;
                case PackageType.CLIENT_DESCRIPTION_PROVIDED:
                    EmegencyDescriptionProvided();
                    break;
            }
            networkStream.BeginRead(buffer, 0, socket.ReceiveBufferSize, OnClientSendMessage, null);
        }

        private void EmergencyInfoRequested()
        {
            ClientPackage package = new ClientPackage(buffer);
            Request req = SSDServer.Instance.requests.FirstOrDefault(req => req.Key.CompareTo(package.RequestInfo.ID) == 0).Value;

            byte[] data = new ServerPackage(ServerPackage.ServerPackageType.RequestInfo, 1, new RequestInfo(req.ID, req.raumnummer, req.standort, req.description), new AccountInfo(Account.ID, Account.Username, new byte[32], Account.Permissions)).ToByteArray();
            networkStream.Write(data, 0, data.Length);
        }

        private void AccountModified()
        {
            if ((Account.Permissions & (int)Account.AccountPermissions.Superuser) == 0 && (Account.Permissions & (int)Account.AccountPermissions.Modify) == 0)
                throw new Exception("[WARNING] Possible malicious package recieved! Âccount modify request received without permission to do so!");

            ClientPackage package = new ClientPackage(buffer);
            Account? toMod = ClientDB.Instance.Accounts.First(acc => acc.ID == package.AccountInfo.ID);
            if (toMod == null)
                throw new Exception(String.Format("Failed to find User with AccountID {0}!", package.AccountInfo.ID));
            toMod.Username = package.AccountInfo.Username;
            toMod.Permissions = package.AccountInfo.Permissions;

            ClientDB.Instance.SaveChanges();
        }

        private void LogoutRequested()
        {
            Account = null;
        }

        private void AcceptEmergencyRequest()
        {
            ClientPackage package = new ClientPackage(buffer);

            if ((Account.Permissions & (int)Account.AccountPermissions.Superuser) == 0 && (package.AccountInfo.Permissions & (int)Account.AccountPermissions.Receiver) == 0)
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
                byte[] err = new ServerPackage(ServerPackage.ServerPackageType.RequestAcknowledged, 0, new RequestInfo(Guid.Empty, "", "", ""), new AccountInfo(0, "", new byte[32], 0)).ToByteArray();
                networkStream.Write(err, 0, err.Length);
                this.ExceptionCatched?.Invoke(this, new Exception(String.Format("Failed login attempt from {0}", socket.Client.RemoteEndPoint)));
                return;
            }
            Account = accInfo;

            byte[] bytes = new ServerPackage(ServerPackage.ServerPackageType.Login, 1, new RequestInfo(Guid.Empty, "", "", ""), new AccountInfo(accInfo.ID, accInfo.Username, new byte[32], accInfo.Permissions)).ToByteArray();
            networkStream.Write(bytes, 0, bytes.Length);
        }

        private void EmergencyRequestInvoked()
        {
            ClientPackage package = new ClientPackage(buffer);

            if ((Account.Permissions & (int)Account.AccountPermissions.Superuser) == 0 && (Account.Permissions & (int)Account.AccountPermissions.Requester) == 0)
            {
                byte[] err = new ServerPackage(ServerPackage.ServerPackageType.RequestAcknowledged, 0, new RequestInfo(Guid.Empty, "", "", ""), new AccountInfo(0, "", new byte[32], 0)).ToByteArray();
                networkStream.Write(err, 0, err.Length);
                throw new InvalidDataException("[WARNING] Possible malicious package recieved! Emergency request made without permission to do so!");
            }

            Guid RequestID = Guid.NewGuid();
            byte[] bytes = new ServerPackage(ServerPackage.ServerPackageType.RequestAcknowledged, 1, new RequestInfo(RequestID, package.RequestInfo.Roomnumber, package.RequestInfo.Location, package.RequestInfo.Description), new AccountInfo(Account.ID, Account.Username, new byte[32], Account.Permissions)).ToByteArray();
            networkStream.Write(bytes, 0, bytes.Length);

            SSDServer.Instance.requests.Add(RequestID, new Request(RequestID, package.RequestInfo.Roomnumber, package.RequestInfo.Location, package.RequestInfo.Description));
            EmergencyRequestMade?.Invoke(this, SSDServer.Instance.requests[RequestID]);
        }

        private void EmegencyDescriptionProvided()
        {
            ClientPackage package = new ClientPackage(buffer);

            if ((Account.Permissions & (int)Account.AccountPermissions.Superuser) == 0 && (Account.Permissions & (int)Account.AccountPermissions.Requester) == 0)
            {
                byte[] err = new ServerPackage(ServerPackage.ServerPackageType.RequestAcknowledged, 0, new RequestInfo(Guid.Empty, "", "", ""), new AccountInfo(0, "", new byte[32], 0)).ToByteArray();
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