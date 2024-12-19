using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDServer.Interfaces
{
    public interface IEmergencyRequest
    {
        public Task<bool> MakeRequest(ClientID id, string raumnummer, string standort);
        public bool SendDescription(ClientID id, string description);
        public bool CheckRequests(ClientID id, out IRequest request);
        public void AcceptRequest(ClientID id, IRequest request);
    }
}
