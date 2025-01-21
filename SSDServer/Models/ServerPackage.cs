using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDServer.Models
{
    public class ServerPackage
    {
        public enum ServerPackageID
        {
            Login = 0,
            Logout = 1,
            RequestReceive = 2,
            RequestAccepted = 3,
            RequestAcknowledged = 4,
            RoleChange = 5,
            AccountInfo = 6,
            RequestInfo = 7
        }

        private ServerPackageID pckID;
        public byte Acknowledge { get; private set; }
        public AccountInfo AccountInfo { get; private set; }
        public RequestInfo RequestInfo { get; private set; }

        public ServerPackage(ServerPackageID id, byte ackowledge, RequestInfo requestInfo, AccountInfo accountInfo)
        {
            pckID = id;
            Acknowledge = ackowledge;
            RequestInfo = requestInfo;
            AccountInfo = accountInfo;
        }

        public byte[] ToByteArray()
        {
            byte[] arr = new byte[] { (byte)pckID };
            arr = arr.Concat(new byte[] { Acknowledge }).ToArray();
            arr = arr.Concat(RequestInfo.ToByteArray()).ToArray();
            arr = arr.Concat(AccountInfo.ToByteArray()).ToArray();
            return arr;
        }
    }
}
