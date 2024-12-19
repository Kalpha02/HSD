using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDServer.Interfaces
{
    public interface IRequest
    {
        public string getRaumnummer();
        public string getStandort();
        public string getDescription();
    }
}
