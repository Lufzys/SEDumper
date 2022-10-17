using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Injector
{
    internal class Program
    {
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        public static void Main()
        {
            Console.Title = "Source Engine Dumper - Injector";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n [SED] This application coded for getting netvars and interfaces easily!");
            Console.WriteLine(" [SED] https://github.com/Lufzys\n");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" Game executable name : ");
            string gameExecutable = Console.ReadLine();
            Console.Write(" ----------------------");
            Console.Write("\n Waiting for process...");

            Process proc = Process.GetProcessesByName(gameExecutable).FirstOrDefault();
            while (proc == null)
            {
                proc = Process.GetProcessesByName(gameExecutable).FirstOrDefault();
                Thread.Sleep(1000);
            }

            var dll = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\SEDumper.dll";
            var injectedLibPtr = proc.LoadLibrary(dll);
            var lib = LoadLibrary(dll);
            var ptr = GetProcAddress(lib, "Entry");

            var diff = IntPtr.Subtract(ptr, (int)lib);
            proc.CallAsync(IntPtr.Add(injectedLibPtr, (int)diff), IntPtr.Zero);
        }
    }
}
