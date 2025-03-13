using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SSDAPI.Interfaces;
using SSDAPI.Models;
using SSDServer.Database;
using SSDServer.Tests.Implementations;

namespace SSDServer.Tests
{
    public class SSDServerTests
    {
        TcpClient client = new TcpClient();
        EmergencyRequester requester;
        public SSDServerTests()
        {
            SSDServer.Instance.ConnectionEstablished += (o, ep) =>
            {
                Console.WriteLine("\tConnection established with endpoint {0}", ep.ToString());
            };
            SSDServer.Instance.ExceptionCatched += (o, e) =>
            {
                Console.WriteLine("\t{0}", e.InnerException.Message);
            };
            SSDServer.Instance.Start(80);

            if (ClientDB.Instance.Accounts.Count() == 0)
            {
                ClientDB.Instance.Accounts.Add(new Account() { PasswordHash = new byte[32], Permissions = 1, Username = "TestUser" });
                ClientDB.Instance.SaveChanges();
            }

            client.Connect(IPAddress.Loopback, 80);
            if (!client.Connected)
                SSDServer.Instance.Close(true);
            requester = new EmergencyRequester(client);
        }

        [Test]
        public void Can_Login_Test()
        {
            if (!client.Connected)
                throw new Exception("Client isn't connected!");

            byte[] data = new ClientPackage(ClientPackage.ClientPackageType.Login, new AccountInfo(0, "TestUser", new byte[32], 1), new RequestInfo(Guid.Empty, "", "", "")).ToByteArray();
            byte[] buffer = new byte[client.ReceiveBufferSize];
            ServerPackage package = null;
            requester.ReceivedPackage += (o, p) =>
            {
                if (p.PackageType == ServerPackage.ServerPackageType.Login)
                    package = p;
            };

            client.GetStream().Write(data, 0, data.Length);
            while(package == null) { }

            Assert.That(package.PackageType == ServerPackage.ServerPackageType.Login);
            Assert.That(package.Acknowledge == 1);
            Assert.That(package.AccountInfo.Username == "TestUser");
            Assert.That(package.AccountInfo.Permissions == 1);

            requester.queuedPackages.Remove(package);
        }

        [Test]
        public void Can_Make_Request_Test()
        {
            if (!client.Connected)
                throw new Exception("Client isn't connected!");

            Task<bool>? t = requester.MakeRequest("001", "Test");
            if (t == null)
                throw new Exception("Returned task was null! Couldn't create request!");

            if (!t.IsCompleted)
                t.Wait();

            if (!t.Result)
                throw new Exception("Server failed to acknowledge emergency request!");

            requester.CheckRequests(out IRequest req);
        }

        [Test]
        public void Can_Receive_Request_Test()
        {
            if (!client.Connected)
                throw new Exception("Client isn't connected!");

            Task<bool>? t = requester.MakeRequest("001", "Test");
            if (t == null)
                throw new Exception("Returned task was null! Couldn't create request!");

            if (!t.IsCompleted)
                t.Wait();

            if (!t.Result)
                throw new Exception("Server failed to acknowledge emergency request!");

            Thread.Sleep(100);

            if (!requester.CheckRequests(out IRequest req))
                throw new Exception("No request received");

            Assert.That(req.getRoomnumber() == "001");
            Assert.That(req.getLocation() == "Test");
        }

        [Test]
        public void Can_Send_Description_Test()
        {
            if (!client.Connected)
                throw new Exception("Client isn't connected!");

            Task<bool>? t = requester.MakeRequest("", "");
            if (t == null)
                throw new Exception("Returned task was null! Couldn't create request!");

            if (!t.IsCompleted)
                t.Wait();

            if (!t.Result)
                throw new Exception("Server failed to acknowledge emergency request!");

            if (!requester.SendDescription("Test"))
                throw new Exception("Failed to send description!");
        }

        [Test]
        public void Can_Receive_Description_Test()
        {
            if (!client.Connected)
                throw new Exception("Client isn't connected!");

            Task<bool>? t = requester.MakeRequest("", "");
            if (t == null)
                throw new Exception("Returned task was null! Couldn't create request!");

            if (!t.IsCompleted)
                t.Wait();

            if (!t.Result)
                throw new Exception("Server failed to acknowledge emergency request!");

            if (!requester.SendDescription("Test"))
                throw new Exception("Failed to send description!");

            if (!requester.CheckRequests(out IRequest req))
                throw new Exception("Failed to receive request!");

            if (req.getDescription() != "Test")
                throw new Exception("Failed to receive emergency description!");
        }
    }
}
