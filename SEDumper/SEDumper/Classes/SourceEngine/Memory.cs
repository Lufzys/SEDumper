using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SEDumper.Classes.SourceEngine
{
    public static unsafe class Memory
    {
        public static IntPtr VirtualAddress(this IntPtr addr, int index) => *(IntPtr*)(*(IntPtr*)(addr) + index * 4);
        public static IntPtr FunctionToPointer<T>(T func) where T : class => Marshal.GetFunctionPointerForDelegate<T>(func);

        unsafe public static void Memcpy(byte[] bytes, IntPtr dest) // faster than Marshal.Copy
        {
            for (int i = 0; i < bytes.Length; ++i)
                *(byte*)(dest + i) = bytes[i];
        }

        unsafe public static byte[] Memcpy(IntPtr source, int len) // faster than Marshal.Copy
        {
            var bytes = new byte[len];

            for (int i = 0; i < len; ++i)
                bytes[i] = *(byte*)(source + i);

            return bytes;
        }
    }

    public static class GenericInterop
    {
        public static Type t_IntPtr = typeof(IntPtr);
        public static Type t_Int32 = typeof(int);
        public static Type t_Int64 = typeof(long);
        public static Type t_Byte = typeof(byte);
        public static Type t_Uint = typeof(uint);
        public static Type t_Vector = typeof(Vector3);
        public static Type t_Bool = typeof(bool);
        public static Type t_Float = typeof(float);
        public static Type t_String = typeof(string);
        public static Type t_Ushort = typeof(ushort);
        public static Type t_Void = null;

        public static T call<T>(this IntPtr addr, params object[] args)
        {
            var tArgs = new Type[args.Length];
            for (var i = 0; i < args.Length; i++)
                tArgs[i] = args[i].GetType();

            var @type = CreateDelegate(typeof(T), tArgs);
            var @delegate = Marshal.GetDelegateForFunctionPointer(addr, @type);
            return (T)@delegate.DynamicInvoke(args);
        }

        public static Delegate deleg(this IntPtr addr, Type type) => Marshal.GetDelegateForFunctionPointer(addr, type);

        public static Delegate WrapVFunc(this IntPtr addr, int index, Type return_Type, params Type[] args)
            => addr.VirtualAddress(index).deleg(CreateDelegate(return_Type, args));

        public static Type CreateDelegate(Type returntype, params Type[] args)
        {
            var temp = args.ToList(); temp.Insert(0, typeof(IntPtr));
            var paramtypes = temp.ToArray();
            var myCurrentDomain = AppDomain.CurrentDomain;
            var myAssemblyName = new AssemblyName() { Name = "TempAssembly" };
            var myAssemblyBuilder = myCurrentDomain.DefineDynamicAssembly(myAssemblyName, AssemblyBuilderAccess.Run);
            var dynamicMod = myAssemblyBuilder.DefineDynamicModule("TempModule");
            var tb = dynamicMod.DefineType("delegate-maker" + Guid.NewGuid(), TypeAttributes.Public | TypeAttributes.Sealed, typeof(MulticastDelegate));

            tb.DefineConstructor(MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.Standard,
                 new Type[] { typeof(object), typeof(IntPtr) }).SetImplementationFlags(MethodImplAttributes.Runtime);
            tb.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig, CallingConventions.Standard, returntype, null,
                             new Type[] { typeof(System.Runtime.CompilerServices.CallConvThiscall) }, paramtypes, null, null).SetImplementationFlags(MethodImplAttributes.Runtime);
            return tb.CreateType();
        }
    }
}
