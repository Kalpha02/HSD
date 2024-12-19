using System;
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
        ClientID id;
        public SSDServerTest()
        {
            SSDServer.Instance.ExceptionCatched += (o, ex) => {
                Console.WriteLine(ex.Message);
            };

            SSDServer.Instance.Start();
        }

        [Test]
        public void Test_Connection()
        {
            TcpClient client = new TcpClient(AddressFamily.InterNetwork);
            client.Connect(IPAddress.Loopback, 16320);
            if (!client.Connected)
                Assert.False();
            client.Close();
        }

        [Test]
        public void Test_RecieveSessionToken()
        {
            TcpClient client = new TcpClient(AddressFamily.InterNetwork);
            client.ReceiveBufferSize = 4096;
            client.SendBufferSize = 4096;

            client.Connect(IPAddress.Loopback, 16320);
            if (!client.Connected)
                Assert.False();

            NetworkStream stream = client.GetStream();
            stream.Write(new byte[] { 0, 3 }, 0, 2); // Send login request

            byte[] buffer = new byte[16];
            if(stream.Read(buffer, 0, buffer.Length) != 16)
                Assert.False();
            id.clientID = new Guid(buffer);
            id.clientType = ClientType.Admin;

            client.Close();
        }

        [Test]
        // This test shows why test-driven development is beneficial, as we now have to test a whole array of functionality instead of testing the individual components
        public void Test_EmergencyRequest()
        {
            TcpClient client = new TcpClient(AddressFamily.InterNetwork);
            client.ReceiveBufferSize = 4096;
            client.SendBufferSize = 4096;

            client.Connect(IPAddress.Loopback, 16320);
            if (!client.Connected)
                Assert.False();

            NetworkStream stream = client.GetStream();
            stream.Write(new byte[] { 0, 3 }, 0, 2); // Send login request as Admin to send and recieve emergency requests

            byte[] buffer = new byte[16];
            if (stream.Read(buffer, 0, buffer.Length) != 16)
                Assert.False();
            id.clientID = new Guid(buffer);
            id.clientType = ClientType.Admin;

            bool recieved = false;
            SSDServer.Instance.RequestReceived += (o, req) => {
                recieved = true;
            };

            EmergencyRequester requester = new EmergencyRequester(client);
            Task<bool> t = requester.MakeRequest(id, "001", "test");

            while (!recieved) // This entire sleep cycle only exists to prevent checking before the request could be processed
                Thread.Sleep(100);

            if(!requester.CheckRequests(id, out IRequest req) || req.getRaumnummer() != "001" || req.getStandort() != "test")
                Assert.False();

            requester.AcceptRequest(id, req);

            if (!t.IsCompleted)
                t.Wait();
            Assert.IsEqual(t.Result, true);

            client.Close();
        }

        ~SSDServerTest()
        {
            SSDServer.Instance.Close(true);
        }

    }
}
