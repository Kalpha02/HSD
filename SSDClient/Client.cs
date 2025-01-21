using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using SSDClient.Models;

namespace SSDServer
{
    public class Client
    {
        AccountInfo accountInfo;
        TcpClient client;
        public NetworkStream stream { get; private set; }
        byte[] buffer;
        const byte Login           = 0;
	    const byte Logout          = 1;
	    const byte RequestReceive  = 2;
	    const byte RequestAccepted = 3;
	    const byte RequestAck      = 4;
	    const byte RoleChange      = 5;
	    const byte AccountInfo     = 6;
        const byte RequestInfo     = 7;


        internal Client() 
        {
            client = new TcpClient(AddressFamily.InterNetwork);
            client.ReceiveBufferSize = 4096;
            client.SendBufferSize = 4096;

            buffer = new byte[client.ReceiveBufferSize];

        }


        public bool connect(IPAddress address) {
            client.Connect(address, 16320);
            if (!client.Connected)
                return false;

            stream = client.GetStream();
            stream.BeginRead(buffer, 0, client.ReceiveBufferSize, onReceiveData, null);
            return true;
        }

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

        private void parseReceivedData()
        {
            switch(buffer[0]) 
            { 
                case Login:
                    login();
                    break;
                case Logout:
                    logout();
                    break;
                case RequestReceive:
                    requestReceive();
                    break;
                case RequestAccepted:
                    requestAccepted();
                    break;
                case RequestAck:
                    requestAck();
                    break;
                case RoleChange:
                    roleChange();
                    break;
                case AccountInfo:
                    rccountInfo();
                    break;
                case RequestInfo:
                    requestInfo();
                    break;


            }
            
        }

        private void requestReceive()
        {
            throw new NotImplementedException();
        }

        private void requestInfo()
        {
            throw new NotImplementedException();
        }

        private void rccountInfo()
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

        public void login()
        {
            /// get account Info from class
            
            ServerPackage package = new ServerPackage(buffer);
            accountInfo = package.AccountInfo;
        }

    }
}
