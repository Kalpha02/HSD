using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDServer.Models
{
    public class RequestInfo
    {
        public Guid ID { get; }
        public string Roomnumber { get; }
        public string Location { get; }
        public string Description { get; }

        public RequestInfo(byte[] data)
        {
            ID = new Guid(data.AsSpan(0, 16));
            int rnLength = BitConverter.ToInt32(data, 16);
            int locationLength = BitConverter.ToInt32(data, 20);
            int descLength = BitConverter.ToInt32(data, 24);

            Roomnumber = Encoding.UTF8.GetString(data.AsSpan(28, rnLength));
            Location = Encoding.UTF8.GetString(data.AsSpan(28 + rnLength, rnLength));
            Description = Encoding.UTF8.GetString(data.AsSpan(28 + rnLength + locationLength, rnLength));
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
            byte[] locationData = Encoding.UTF8.GetBytes(Location);
            byte[] descData = Encoding.UTF8.GetBytes(Description);

            byte[] bytes = BitConverter.GetBytes(rnData.Length);
            bytes = bytes.Concat(BitConverter.GetBytes(locationData.Length)).ToArray();
            bytes = bytes.Concat(BitConverter.GetBytes(descData.Length)).ToArray();
            bytes = bytes.Concat(ID.ToByteArray()).ToArray();
            bytes = bytes.Concat(rnData).ToArray();
            bytes = bytes.Concat(locationData).ToArray();
            bytes = bytes.Concat(descData).ToArray();

            return bytes;
        }
    }
}
