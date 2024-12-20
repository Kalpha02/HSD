using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SSDServer.Tests.Extensions
{
    public static class SocketExtensions
    {
        public static bool IsDisposed(this Socket s)
        {
            if (s == null)
                return true;
            return (bool)s.GetType().GetProperty("Disposed", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty).GetValue(s);
        }
    }
}
