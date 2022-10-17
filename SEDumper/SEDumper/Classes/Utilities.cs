using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SEDumper.Classes
{
    public static class Utilities
    {
        public static (IntPtr, int) Module(this string moduleName)
        {
            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            {
                if (module.ModuleName == moduleName)
                    return (module.BaseAddress, module.ModuleMemorySize);
            }
            return (IntPtr.Zero, 0);
        }

        public static string ToMemString(this IntPtr ptr) => Marshal.PtrToStringAnsi(ptr);

        public static int GetAllClassesIndex(string interfaceName)
        {
            switch (interfaceName)
            {
                case "VClient016": // Source Engine 2007
                    return 7;

                case "VClient017":  // Source Engine 2013
                    return 8;
                default:
                    return 9;
            }
        }
    }
}
