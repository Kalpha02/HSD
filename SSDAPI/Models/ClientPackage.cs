using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SSDAPI.Models
{
    public class ClientPackage
    {
        public enum ClientPackageType
        { 
            Login = 0,
            Logout = 1,
            Request = 2,
            RequestAccepted = 3,
            AccoutnModified = 4,
            RequestInfo = 5,
            RequestDescription = 6
        }

        public ClientPackageType PackageType { get; }
        public AccountInfo AccountInfo { get; private set; }
        public RequestInfo RequestInfo { get; private set; }
        public ClientPackage(byte[] data)
        {
            PackageType = (ClientPackageType)data[0];
            int rnLength = BitConverter.ToInt32(data, 1);
            int locLength = BitConverter.ToInt32(data, 5);
            int descLength = BitConverter.ToInt32(data, 9);
            RequestInfo = new RequestInfo(data.AsSpan(1, 28 + rnLength + locLength + descLength).ToArray());
            AccountInfo = new AccountInfo(data.AsSpan(29 + rnLength + locLength + descLength, data.Length - (29 + rnLength + locLength + descLength)).ToArray());
        }

        public ClientPackage(ClientPackageType type, AccountInfo accountInfo, RequestInfo requestInfo)
        {
            this.PackageType = type;
            this.AccountInfo = accountInfo;
            this.RequestInfo = requestInfo;
        }

        public byte[] ToByteArray()
        {
            byte[] data = new byte[] { (byte)PackageType };
            data = data.Concat(RequestInfo.ToByteArray()).ToArray();
            return data.Concat(AccountInfo.ToByteArray()).ToArray();
        }
    }
}
