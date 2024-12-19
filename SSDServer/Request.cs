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

        public Guid RequestID { get; private set; }

        public Request(string raumnummer, string standort, string desciption = "")
        {
            this.raumnummer = raumnummer;
            this.standort = standort;
            RequestID = Guid.NewGuid();
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
    }
}
