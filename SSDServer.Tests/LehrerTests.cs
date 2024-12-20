using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SSDServer.Tests.Implementations;

namespace SSDServer.Tests
{
    public class LehrerTests
    {
        public LehrerTests() { }

        [Test]
        public void Test_EmergencyRequestSuccess()
        {
            SSDServer.Instance.Start();
            TcpClient client = new TcpClient(AddressFamily.InterNetwork);
            client.ReceiveBufferSize = 4096;
            client.SendBufferSize = 4096;

            client.Connect(IPAddress.Loopback, 16320);
            if (!client.Connected)
                Assert.False();

            NetworkStream stream = client.GetStream();
            stream.Write(new byte[] { 0, 2 }, 0, 2); // Send login request as Lehrer to succeed the emergency request

            byte[] buffer = new byte[16];
            if (stream.Read(buffer, 0, buffer.Length) != 16)
                Assert.False();
            ClientID id;
            id.clientID = new Guid(buffer);
            id.clientType = ClientType.Admin;

            EmergencyRequester requester = new EmergencyRequester(client);
            Task<bool>? t = requester.MakeRequest(id, "001", "test");

            if (t == null)
                Assert.False();
            client.Close();
            SSDServer.Instance.Close(true);
        }
    }
}
