using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDClient.Models
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

        public ServerPackage(byte[] data) {
            Acknowledge = data[1];
            int roomNumber = BitConverter.ToInt32(data, 2);
            int location = BitConverter.ToInt32(data, 6);
            int description = BitConverter.ToInt32(data, 10);
            RequestInfo = new RequestInfo(new Guid(data.AsSpan(11, 16).ToArray()),
                Encoding.UTF8.GetString(data.AsSpan(27, roomNumber).ToArray()),
                Encoding.UTF8.GetString(data.AsSpan(27 + roomNumber, location).ToArray()),
                Encoding.UTF8.GetString(data.AsSpan(27 + roomNumber + location, description).ToArray())
                );
            int offset = 27 + roomNumber + location + description;
            AccountInfo = new AccountInfo(data.AsSpan(offset, data.Length - offset).ToArray());


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
