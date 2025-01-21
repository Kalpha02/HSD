using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDServer
{
    public class Request
    {
        public string raumnummer, standort, description;
        public Guid id;

        public Request(Guid id, string raumnummer, string standort, string desciption = "")
        {
            this.id = id;
            this.raumnummer = raumnummer;
            this.standort = standort;
            this.description = desciption;
        }
    }
}
