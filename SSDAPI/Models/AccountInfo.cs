using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDAPI.Models
{
    public class AccountInfo
    {
        public long ID { get; private set; }
        public string Username { get; private set; }
        public byte[] PasswordHash { get; private set; }
        public int Permissions { get; private set; }
        private byte[] arr;
        public AccountInfo(byte[] data)
        {
            ID = BitConverter.ToInt64(data, 0);
            int unLength = BitConverter.ToInt32(data, 8);
            PasswordHash = data.AsSpan(12, 32).ToArray();
            Permissions = BitConverter.ToInt32(data, 44);
            Username = Encoding.UTF8.GetString(data, 48, data.Length - 48);
            arr = data;
        }

        public AccountInfo(long id, string username, int permissions)
        {
            ID = id;
            Username = username;
            PasswordHash = new byte[32];
            Permissions = permissions;

            arr = BitConverter.GetBytes(id);
            arr = arr.Concat(BitConverter.GetBytes(username.Length)).ToArray();
            arr = arr.Concat(PasswordHash).ToArray();
            arr = arr.Concat(BitConverter.GetBytes(Permissions)).ToArray();
        }

        public byte[] ToByteArray()
        {
            return arr;
        }
    }
}
