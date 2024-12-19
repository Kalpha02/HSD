using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SSDServer.Tests.Extensions
{
    public static class AssemblyExtensions
    {
        public static bool IsAssemblyDebugBuild(this Assembly assembly)
        {
            return assembly.GetCustomAttributes(false).OfType<DebuggableAttribute>().Any(da => da.IsJITTrackingEnabled);
        }
    }
}
