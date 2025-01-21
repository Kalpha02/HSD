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
            Superuser = 0,
            Receiver = 1,
            Requester = 2,
            Create = 4,
            Modify = 8,
            Delete = 16
        }

    }
}
