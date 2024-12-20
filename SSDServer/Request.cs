using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSDServer.Interfaces;

namespace SSDServer
{
    public class Request : IRequest
    {
        private string raumnummer, standort, description;
        ClientID requestee;

        public Request(ClientID requestee, string raumnummer, string standort, string desciption = "")
        {
            this.raumnummer = raumnummer;
            this.standort = standort;
            this.requestee = requestee;
        }

        public string getRaumnummer()
        {
            return raumnummer;
        }

        public string getStandort()
        {
            return standort;
        }

        public string getDescription()
        {
            return description;
        }

        public ClientID getRequestee()
        {
            return requestee;
        }
    }
}
