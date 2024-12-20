using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SSDServer.Tests.Implementations;
using SSDServer.Interfaces;

namespace SSDServer.Tests
{
    public class AdminTests
    {
        public AdminTests() { }

        [Test]
        public void Test_EmegergencySendAndAccept()
        {
            // Establish connection
            SSDServer.Instance.Start();
            TcpClient client = new TcpClient(AddressFamily.InterNetwork);
            client.ReceiveBufferSize = 4096;
            client.SendBufferSize = 4096;

            client.Connect(IPAddress.Loopback, 16320);
            if (!client.Connected)
            {
                SSDServer.Instance.Close(true);
                Assert.False();
            }

            // Send login request as Admin
            NetworkStream stream = client.GetStream();
            stream.Write(new byte[] { 0, 3 }, 0, 2);

            byte[] buffer = new byte[16];
            if (stream.Read(buffer, 0, buffer.Length) != 16)
            {
                SSDServer.Instance.Close(true);
                Assert.False();
            }
            ClientID id;
            id.clientID = new Guid(buffer);
            id.clientType = ClientType.Admin;

            // Make a request
            EmergencyRequester requester = new EmergencyRequester(client);
            Task<bool>? t = requester.MakeRequest(id, "001", "test");

            if (t == null)
            {
                SSDServer.Instance.Close(true); // Request wasn't recognized
                Assert.False();
            }

            if (!requester.CheckRequests(id, out IRequest req))
            {
                SSDServer.Instance.Close(true); // No requests available although one was send
                Assert.False();
            }
            Assert.That(req.getStandort() == "test");
            Assert.That(req.getRaumnummer() == "001");

            requester.AcceptRequest(id, req);

            if (!t.IsCompleted)
                t.Wait();
            Assert.That(t.Result); // Check that the request was successfully accepted

            client.Close();
            SSDServer.Instance.Close(true);
        }
    }
}
