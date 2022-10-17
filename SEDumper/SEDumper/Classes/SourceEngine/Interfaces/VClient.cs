using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SEDumper.Classes.SourceEngine.GenericInterop;

namespace SEDumper.Classes.SourceEngine.Interfaces
{
    public class VClient : BaseInterface
    {
        Delegate getAllClasses;

        public VClient((IntPtr, IntPtr, int) infos) : base(infos)
        {
            getAllClasses = WrapVFunc(Utilities.GetAllClassesIndex(InterfaceManager.GetInterfaceVersion("VClient")), t_IntPtr);
        }

        unsafe public ClientClass* GetAllClasses() => (ClientClass*)(IntPtr)getAllClasses.DynamicInvoke(Address);
    }
}
