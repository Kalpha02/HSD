﻿using System;
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
        public AccountInfo(byte[] data)
        {
            ID = BitConverter.ToInt64(data, 0);
            int unLength = BitConverter.ToInt32(data, 8);
            PasswordHash = data.AsSpan(12, 32).ToArray();
            Permissions = BitConverter.ToInt32(data, 44);
            Username = Encoding.UTF8.GetString(data, 48, unLength);
        }

        public AccountInfo(long id, string username, byte[] password, int permissions)
        {
            ID = id;
            Username = username;
            PasswordHash = password;
            Permissions = permissions;
        }

        public byte[] ToByteArray()
        {
            byte[] unData = Encoding.UTF8.GetBytes(Username);
            byte[] data = BitConverter.GetBytes(ID);
            data = data.Concat(BitConverter.GetBytes(unData.Length)).ToArray();
            data = data.Concat(PasswordHash).ToArray();
            data = data.Concat(BitConverter.GetBytes(Permissions)).ToArray();
            data = data.Concat(unData).ToArray();

            return data;
        }
    }
}
