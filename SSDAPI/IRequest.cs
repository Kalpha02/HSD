using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDAPI
{
    public interface IRequest
    {
        public string getRoomnumber();
        public string getLocation();
        public string getDescription();
    }
}
