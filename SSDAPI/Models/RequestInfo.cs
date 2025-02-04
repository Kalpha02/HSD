using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDAPI.Models
{
    public class RequestInfo
    {
        public Guid ID { get; }
        public string Roomnumber { get; }
        public string Location { get; }
        public string Description { get; }

        public RequestInfo(byte[] data)
        {
            int rnLength = BitConverter.ToInt32(data, 0);
            int locLength = BitConverter.ToInt32(data, 4);
            int descLength = BitConverter.ToInt32(data, 8);
            ID = new Guid(data.AsSpan(12, 16));
            Roomnumber = Encoding.UTF8.GetString(data, 28, rnLength);
            Location = Encoding.UTF8.GetString(data, 28 + rnLength, locLength);
            Description = Encoding.UTF8.GetString(data, 28 + rnLength + locLength, descLength);
        }

        public RequestInfo(Guid id, string rn, string location, string desc)
        {
            ID = id;
            Roomnumber = rn;
            Location = location;
            Description = desc;
        }

        public byte[] ToByteArray()
        {
            byte[] rnData = Encoding.UTF8.GetBytes(Roomnumber);
            byte[] locData = Encoding.UTF8.GetBytes(Location);
            byte[] descData = Encoding.UTF8.GetBytes(Description);

            byte[] bytes = BitConverter.GetBytes(rnData.Length);
            bytes = bytes.Concat(BitConverter.GetBytes(locData.Length)).ToArray();
            bytes = bytes.Concat(BitConverter.GetBytes(descData.Length)).ToArray();
            bytes = bytes.Concat(ID.ToByteArray()).ToArray();
            bytes = bytes.Concat(rnData).ToArray();
            bytes = bytes.Concat(locData).ToArray();
            bytes = bytes.Concat(descData).ToArray();

            return bytes;
        }
    }
}
