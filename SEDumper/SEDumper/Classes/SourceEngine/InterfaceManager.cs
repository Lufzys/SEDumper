using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SEDumper.Classes.SourceEngine
{
    internal class InterfaceManager
    {
        delegate IntPtr DelCreateInterface(string interfaceName, out int returnCode);

        public static IntPtr GetInterfaceAddress(string moduleName, string interfaceName)
        {
            string InterfaceName = GetInterfaceVersion(interfaceName);

            IntPtr interfaceFuncAddr = Win32.GetProcAddress((IntPtr)moduleName.Module().Item1, "CreateInterface");
            DelCreateInterface interfaceFunc = (DelCreateInterface)Marshal.GetDelegateForFunctionPointer(interfaceFuncAddr, typeof(DelCreateInterface));
            return interfaceFunc(InterfaceName, out int zero);
        }

        public static (IntPtr, IntPtr, int) GetInterface(string moduleName, string interfaceName)
        {
            return (GetInterfaceAddress(moduleName, interfaceName), moduleName.Module().Item1, moduleName.Module().Item2);
        }

        public static string GetInterfaceVersion(string interfaceName)
        {
            foreach (var item in Interfaces)
                if (item.Name.StartsWith(interfaceName + "0"))
                    return item.Name;

            return interfaceName;
        }

        unsafe public struct InterfaceReg
        {
            public fixed byte m_CreateFn[4];
            public char* m_pName;
            public InterfaceReg* m_pNext;
        };

        public struct Interface
        {
            public string Module;
            public string Name;
        }
        public static List<Interface> Interfaces = new List<Interface>();
        public unsafe static void Dump(string moduleName)
        {
            IntPtr CreateInterfaceFunc = Win32.GetProcAddress(moduleName.Module().Item1, "CreateInterface");

            if (CreateInterfaceFunc != IntPtr.Zero)
            {
                uint var01 = (((uint)CreateInterfaceFunc + 0x8));
                if (var01 == 0) return;

                ushort var02 = *(ushort*)((uint)CreateInterfaceFunc + 0x5);
                if (var02 == 0) return;

                ushort var03 = *(ushort*)((uint)CreateInterfaceFunc + 0x7);
                if (var03 == 0) return;

                uint var04 = (uint)(var01 + (var02 - var03));
                if (var04 == 0) return;

                InterfaceReg* interface_registry = **(InterfaceReg***)(var04 + 0x6);
                if (interface_registry == null) return;

                for (InterfaceReg* pCur = interface_registry; (IntPtr)pCur != IntPtr.Zero; pCur = pCur->m_pNext)
                {
                    Interface ınterface = new Interface();
                    ınterface.Module = moduleName;
                    ınterface.Name = Marshal.PtrToStringAnsi((IntPtr)pCur->m_pName);
                    Interfaces.Add(ınterface);
                }
            }
        }

        public static void DumpAllInterfaces()
        {
            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            {
                if (!module.ModuleName.Contains(".dll"))
                    continue;

                if (module.ModuleName == "crashhandler.dll")
                    continue;

                if (module.ModuleName == "hw.dll")
                    continue;

                if (module.ModuleName == "steamclient.dll")
                    continue;

                if (module.ModuleName == "vstdlib_s.dll")
                    continue;

                if (module.ModuleName.Contains("mscore"))
                    continue;

                Dump(module.ModuleName);
            }
        }
    }
}
