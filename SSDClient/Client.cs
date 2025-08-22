using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using SSDAPI.Models;

namespace SSDServer
{
    public class Client
    {
        AccountInfo? accountInfo = null;
        TcpClient client;
        public NetworkStream stream { get; private set; }
        byte[] buffer;


        internal Client() 
        {
            client = new TcpClient(AddressFamily.InterNetwork);
            client.ReceiveBufferSize = 4096;
            client.SendBufferSize = 4096;
            buffer = new byte[client.ReceiveBufferSize];

        }

        /// <summary>
        /// Connects to server and triggers "onRecieveData" if data recieved.
        /// </summary>
        /// <param name="address"></param>
        /// <returns>True if connection was succesfull.</returns>
        public bool connect(IPAddress address)
        {
            try
            {
                client.Connect(address, 16320);
            }
            catch (Exception)
            {

                throw;
            }
            if (!client.Connected)
                return false;

            stream = client.GetStream();
            stream.BeginRead(buffer, 0, client.ReceiveBufferSize, onReceiveData, null);
            return true;
        }

        /// <summary>
        /// Tries to parse recieved data.
        /// </summary>
        /// <param name="ar"></param>
        private void onReceiveData(IAsyncResult ar)
        {
            try
            {
                parseReceivedData();
            }
            catch(Exception e)
            { 
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Parses the recieved data.
        /// </summary>
        /// <returns>True if parsing is succesfull.</returns>
        private bool parseReceivedData()
        {
            switch((ServerPackage.ServerPackageType)buffer[0])
            { 
                case ServerPackage.ServerPackageType.Login:
                    login();
                    break;
                case ServerPackage.ServerPackageType.Logout:
                    logout();
                    break;
                case ServerPackage.ServerPackageType.RequestReceive:
                    requestReceive();
                    break;
                case ServerPackage.ServerPackageType.RequestAccepted:
                    requestAccepted();
                    break;
                case ServerPackage.ServerPackageType.RequestAcknowledged:
                    requestAck();
                    break;
                case ServerPackage.ServerPackageType.RoleChange:
                    roleChange();
                    break;
                case ServerPackage.ServerPackageType.AccountInfo:
                    rccountInfo();
                    break;
                case ServerPackage.ServerPackageType.RequestInfo:
                    requestInfo();
                    break;
                default:
                    return false;

            }
            return true;
            
        }

        private void requestReceive()
        {
            throw new NotImplementedException();
        }

        private void requestInfo()
        {
            throw new NotImplementedException();
        }

        private void rccountInfo() //accountInfo()?
        {
            throw new NotImplementedException();
        }

        private void roleChange()
        {
            throw new NotImplementedException();
        }

        private void requestAck()
        {
            throw new NotImplementedException();
        }

        private void requestAccepted()
        {
            throw new NotImplementedException();
        }

        private void logout()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Logs into account. Throws an exception if it not acknowledged.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void login()
        {
            /// get account Info from class
            ServerPackage package = new ServerPackage(buffer);
            if (package.Acknowledge == 0)
            {
                throw new InvalidOperationException("Failed to login.");
            }
            accountInfo = package.AccountInfo;
        }
    }
}
