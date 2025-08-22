using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSDAPI.Interfaces;

namespace SSDServer
{
    internal class Request : IRequest
    {
        public string raumnummer, standort, description;
        public Guid ID { get; set; }

        internal Request(Guid id, string raumnummer, string standort, string desciption = "")
        {
            this.ID = id;
            this.raumnummer = raumnummer;    //TODO: adapt to other locations as "corridor", "foyer" or "schoolyard"
            this.standort = standort;
            this.description = desciption;
        }

        public string getRoomnumber() //TODO: adapt to other locations as "corridor", "foyer" or "schoolyard"
        {
            return raumnummer;
        }

        public string getLocation()
        {
            return standort;
        }

        public string getDescription()
        {
            return description;
        }
    }
}
