using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDAPI.Models
{
    public class ServerPackage
    {
        public enum ServerPackageType
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

        public ServerPackageType PackageType { get; }
        public byte Acknowledge { get; private set; }
        public AccountInfo AccountInfo { get; private set; }
        public RequestInfo RequestInfo { get; private set; }

        public ServerPackage(ServerPackageType id, byte ackowledge, RequestInfo requestInfo, AccountInfo accountInfo)
        {
            PackageType = id;
            Acknowledge = ackowledge;
            RequestInfo = requestInfo;
            AccountInfo = accountInfo;
        }

        public ServerPackage(byte[] data)
        {
            // Injection protection necessary

            PackageType = (ServerPackageType)data[0];
            Acknowledge = data[1];
            int roomNumber = BitConverter.ToInt32(data, 2);
            int location = BitConverter.ToInt32(data, 6);
            int description = BitConverter.ToInt32(data, 10);
            RequestInfo = new RequestInfo(data.AsSpan(2, 28 + roomNumber + location + description).ToArray());
            int offset = 30 + roomNumber + location + description;
            AccountInfo = new AccountInfo(data.AsSpan(offset, data.Length - offset).ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            byte[] arr = new byte[] { (byte)PackageType, Acknowledge };
            arr = arr.Concat(RequestInfo.ToByteArray()).ToArray();
            arr = arr.Concat(AccountInfo.ToByteArray()).ToArray();
            return arr;
        }
    }
}