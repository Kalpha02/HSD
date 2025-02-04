using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDServer.Database
{
    public class Account
    {
        public long ID { get; set; } = 0;
        public string Username { get; set; } = "";
        public byte[] PasswordHash { get; set; } = new byte[32];
        public int Permissions { get; set; } = 0;

        public enum AccountPermissions
        {
            Superuser = 1,
            Receiver = 2,
            Requester = 4,
            Create = 8,
            Modify = 16,
            Delete = 32
        }

    }
}
