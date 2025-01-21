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

        public RequestInfo(Guid id, string rn, string location, string desc)
        {
            ID = id;
            Roomnumber = rn;
            Location = location;
            Description = desc;
        }

        public byte[] ToByteArray()
        {
            byte[] bytes = BitConverter.GetBytes(Roomnumber.Length);
            bytes = bytes.Concat(BitConverter.GetBytes(Location.Length)).ToArray();
            bytes = bytes.Concat(BitConverter.GetBytes(Description.Length)).ToArray();
            bytes = bytes.Concat(ID.ToByteArray()).ToArray();
            bytes = bytes.Concat(Encoding.UTF8.GetBytes(Roomnumber)).ToArray();
            bytes = bytes.Concat(Encoding.UTF8.GetBytes(Location)).ToArray();
            bytes = bytes.Concat(Encoding.UTF8.GetBytes(Description)).ToArray();

            return bytes;
        }
    }
}
