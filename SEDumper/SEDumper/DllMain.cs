using SEDumper.Classes;
using SEDumper.Classes.SourceEngine;
using SEDumper.Classes.SourceEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SEDumper
{
    public static class DllMain
    {
        public static VClient Client;

        [DllExport("Entry")]
        public static void EntryPoint()
        {
            Win32.AllocConsole();
            Console.Title = "SEDumper";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n [SED] Source Engine Dumper - https://github.com/Lufzys\n");
            Console.ForegroundColor = ConsoleColor.Green;

            #region are all modules is loaded? BRUHH SOLUTION
            bool isServerBrowserLoaded = false;
            findServerBrowser:
            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            {
                if (module.ModuleName.ToLower() == "serverbrowser.dll".ToLower())
                    isServerBrowserLoaded = true;
            }
            if (!isServerBrowserLoaded)
            {
                Thread.Sleep(500);
                goto findServerBrowser;
            }
            #endregion

            #region Dump Interfaces
            InterfaceManager.DumpAllInterfaces();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm")}] SEDumper (Interfaces) - https://github.com/Lufzys");
            sb.AppendLine();
            string lastModule = string.Empty;
            foreach (var item in InterfaceManager.Interfaces)
            {
                if(lastModule != item.Module)
                {
                    if (lastModule != string.Empty)
                        sb.AppendLine();
                    lastModule = item.Module;
                    sb.AppendLine($"[{item.Module}]");
                    sb.AppendLine($"{item.Name} : 0x{((int)InterfaceManager.GetInterfaceAddress(item.Module, item.Name) - (int)item.Module.Module().Item1).ToString("X")}");
                }
                else
                    sb.AppendLine($"{item.Name} : 0x{((int)InterfaceManager.GetInterfaceAddress(item.Module, item.Name) - (int)item.Module.Module().Item1).ToString("X")}");
            }
            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) +$"\\{Process.GetCurrentProcess().ProcessName}_Interfaces.txt", sb.ToString());
            Console.WriteLine($" [SED] Interfaces saved into Desktop->{Process.GetCurrentProcess().ProcessName}_Interfaces.txt");
            #endregion

            #region Dump Netvars
            Client = new VClient(InterfaceManager.GetInterface("client.dll", "VClient"));
            NetvarManager NetvarMngr = new NetvarManager();

            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $"\\{Process.GetCurrentProcess().ProcessName}_Netvars.txt", NetvarMngr.DumpClasses());
            Console.WriteLine($" [SED] Netvars saved into Desktop->{Process.GetCurrentProcess().ProcessName}_Netvars.txt");
            #endregion
        }
    }
}
