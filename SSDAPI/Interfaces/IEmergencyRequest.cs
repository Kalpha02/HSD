using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDAPI.Interfaces
{
    public interface IEmergencyRequest
    {
        public Task<bool>? MakeRequest(string raumnummer, string standort);
        public bool SendDescription(string description);
        public bool CheckRequests(out IRequest request);
        public void AcceptRequest(IRequest request);
    }
}