﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SSDServer.Tests.Extensions;
using SSDServer.Tests;
using System.Net;
using SSDServer.Tests.Implementations;
using System.Data;
using SSDServer.Interfaces;
using System.IO;

namespace SSDServer.Tests
{
    public class SSDServerTest
    {
        public SSDServerTest() { }

        [Test]
        public void Test_RecieveSessionToken()
        {
            SSDServer.Instance.Start();
            TcpClient client = new TcpClient(AddressFamily.InterNetwork);
            client.ReceiveBufferSize = 4096;
            client.SendBufferSize = 4096;

            client.Connect(IPAddress.Loopback, 16320);
            if (!client.Connected)
                Assert.False();

            NetworkStream stream = client.GetStream();
            stream.Write(new byte[] { 0, 0 }, 0, 2); // Send login request

            byte[] buffer = new byte[16];
            if(stream.Read(buffer, 0, buffer.Length) != 16)
                Assert.False();
            ClientID id;
            id.clientID = new Guid(buffer);
            id.clientType = ClientType.Admin;

            client.Close();
            SSDServer.Instance.Close(true);
        }
    }
}
