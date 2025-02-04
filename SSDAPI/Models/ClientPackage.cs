using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDClient.Models
{
    public class ClientPackage
    {
        public AccountInfo AccountInfo { get; private set; }
        public RequestInfo RequestInfo { get; private set; }
        public ClientPackage(byte[] data)
        {
            int rnLength = BitConverter.ToInt32(data, 0);
            int locLength = BitConverter.ToInt32(data, 4);
            int descLength = BitConverter.ToInt32(data, 8);
            Guid id = new Guid(data.AsSpan(12, 16));
            string rn = Encoding.UTF8.GetString(data, 28, rnLength);
            string location = Encoding.UTF8.GetString(data, 28 + rnLength, locLength);
            string desc = Encoding.UTF8.GetString(data, 28 + rnLength + locLength, descLength);
            RequestInfo = new RequestInfo(id, rn, location, desc);
            AccountInfo = new AccountInfo(data.AsSpan(28, data.Length - (28 + rnLength + locLength + descLength)).ToArray());
        }
    }
}
