using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDAPI.Interfaces
{
    public interface IRequest
    {
        public Guid ID { get; }
        public string getRoomnumber();
        public string getLocation();
        public string getDescription();
    }
}
