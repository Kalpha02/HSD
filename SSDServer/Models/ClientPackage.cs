using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDServer.Models
{
    public class ClientPackage
    {
        public enum ClientPackageID
        {
            Login = 0,
            Logout = 1,
            Request = 2,
            RequestAccepted = 3,
            AccountModified = 4,
            RequestedInfo = 5,
            RequestDescription = 6,
        }

        private ClientPackageID packageID;
        public AccountInfo AccountInfo { get; private set; }
        public RequestInfo RequestInfo { get; private set; }
        public ClientPackage(byte[] data)
        {
            packageID = (ClientPackageID)data[0];
            int rnLength = BitConverter.ToInt32(data, 1);
            int locLength = BitConverter.ToInt32(data, 5);
            int descLength = BitConverter.ToInt32(data, 9);
            Guid id = new Guid(data.AsSpan(13, 16));
            string rn = Encoding.UTF8.GetString(data, 29, rnLength);
            string location = Encoding.UTF8.GetString(data, 29 + rnLength, locLength);
            string desc = Encoding.UTF8.GetString(data, 29 + rnLength + locLength, descLength);
            RequestInfo = new RequestInfo(id, rn, location, desc);
            AccountInfo = new AccountInfo(data.AsSpan(29, data.Length - (29 + rnLength + locLength + descLength)).ToArray());
        }

        public ClientPackage(ClientPackageID packageID, RequestInfo req, AccountInfo acc)
        {
            this.packageID = packageID;
            RequestInfo = req;
            AccountInfo = acc;
        }

        public byte[] ToByteArray()
        {
            byte[] data = new byte[] { (byte)packageID };
            data = RequestInfo.ToByteArray();
            return data.Concat(AccountInfo.ToByteArray()).ToArray();
        }
    }
}
