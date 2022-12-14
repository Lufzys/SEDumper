using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static SEDumper.Classes.Utilities;

namespace SEDumper.Classes.SourceEngine
{
    public unsafe class NetvarManager
    {
        List<netvar_table> database;
        public long count = 0;

        public NetvarManager()
        {
            database = new List<netvar_table>();

            for (ClientClass* pClass = DllMain.Client.GetAllClasses(); (IntPtr)pClass != IntPtr.Zero; pClass = pClass->m_pNext)
            {
                var recvTable = pClass->m_pRecvTable;
                if ((IntPtr)recvTable != IntPtr.Zero)
                    database.Add(LoadTable(recvTable));
            }
        }

        private netvar_table LoadTable(RecvTable* recvTable)
        {
            var table = new netvar_table();
            table.child_props = new List<IntPtr>();
            table.child_tables = new List<netvar_table>();
            table.offset = 0;
            table.name = Marshal.PtrToStringAnsi((IntPtr)recvTable->m_pNetTableName);

            for (int i = 0; i < recvTable->m_nProps; i++)
            {
                count++;
                var pProp = &recvTable->m_pProps[i];
                if ((IntPtr)pProp == IntPtr.Zero || char.IsDigit(Marshal.PtrToStringAnsi((IntPtr)pProp->m_pVarName)[0]))
                    continue;
                if (Marshal.PtrToStringAnsi((IntPtr)pProp->m_pVarName) == "baseclass")
                    continue;

                if (pProp->m_RecvType == 6 && (IntPtr)pProp->m_pDataTable != IntPtr.Zero)
                {
                    table.child_tables.Add(LoadTable(pProp->m_pDataTable));
                    var last = table.child_tables.Last();
                    last.offset = pProp->m_Offset;
                    last.prop = pProp;
                    table.child_tables[table.child_tables.Count - 1] = last;
                }
                else
                    table.child_props.Add((IntPtr)pProp);
            }

            return table;
        }

        public int GetOffset(string tablename, string varname)
        {
            foreach (var table in database)
                if (table.name == tablename)
                    return GetOffset(table, varname);
            return -1;
        }

        public static int offset_fromchild = -1;
        private int GetOffset(netvar_table table, string varname, bool from_child = false)
        {
            foreach (RecvProp* props in table.child_props)
                if (Marshal.PtrToStringAnsi((IntPtr)props->m_pVarName) == varname)
                    return table.offset + props->m_Offset;
            foreach (var child_tables in table.child_tables)
            {
                var ret = GetOffset(child_tables, varname, true);
                if (ret != -1) return table.offset + ret;
            }
            foreach (var child_tables in table.child_tables)
                if (Marshal.PtrToStringAnsi((IntPtr)child_tables.prop->m_pVarName) == varname)
                    return table.offset + child_tables.offset;

            return -1;
        }

        #region Dump Netvar Table to Desktop
        StringBuilder sb = new StringBuilder();
        private void DumpClasses(netvar_table table, string parent_table_name, bool from_child = false)
        {
            if (table.child_props.Count == 0) return;
            if (!from_child) 
                sb.AppendLine($"[{table.name}]");

            foreach (RecvProp* props in table.child_props)
            {
                var prop_name = ((IntPtr)props->m_pVarName).ToMemString();
                sb.AppendLine($"{prop_name} = 0x{props->m_Offset.ToString("X")}");
            }
            foreach (var child_tables in table.child_tables)
            {
                DumpClasses(child_tables, parent_table_name, true);
                var mprop_name = ((IntPtr)child_tables.prop->m_pVarName).ToMemString();
                sb.AppendLine($"{mprop_name} = 0x{child_tables.prop->m_Offset.ToString("X")}");
            }
            sb.AppendLine();
        }

        public string DumpClasses()
        {
            sb.Clear();
            sb.AppendLine($"[{DateTime.Now.ToString("MM/dd/yyyy HH:mm")}] SEDumper (Netvars) - https://github.com/Lufzys");
            sb.AppendLine();
            foreach (var table in database)
                DumpClasses(table, table.name);
            return sb.ToString();
        }
        #endregion
    }

    unsafe public struct netvar_table
    {
        public string name;
        public RecvProp* prop;
        public int offset;
        public List<IntPtr> child_props;
        public List<netvar_table> child_tables;
    };

    unsafe public struct CRecvProxyData
    {
        RecvProp* m_pRecvProp;
        public DVariant m_Value;
        int m_iElement;
        int m_ObjectID;
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct DVariant
    {
        [FieldOffset(0)]
        float m_Float;
        [FieldOffset(0)]
        public int m_Int;
        [FieldOffset(0)]
        char* m_pString;
        [FieldOffset(0)]
        void* m_pData;
        [FieldOffset(0)]
        fixed float m_Vector[3];
        [FieldOffset(0)]
        long m_Int64;
        [FieldOffset(0)]
        int m_Type;
    }

    unsafe public struct RecvProp
    {
        public char* m_pVarName;
        public int m_RecvType;
        int m_Flags;
        int m_StringBufferSize;
        bool m_bInsideArray;
        void* m_pExtraData;
        RecvProp* m_pArrayProp;
        void* m_ArrayLengthProxy;
        public IntPtr m_ProxyFn;
        void* m_DataTableProxyFn;
        public RecvTable* m_pDataTable;
        public int m_Offset;
        int m_ElementStride;
        int m_nElements;
        char* m_pParentArrayPropName;
    };

    unsafe public struct RecvTable
    {
        public RecvProp* m_pProps;
        public int m_nProps;
        void* m_pDecoder;
        public char* m_pNetTableName;
        fixed byte pad00[2];
    };

    unsafe public struct ClientClass
    {
        public IntPtr m_pCreateFn;
        public IntPtr m_pCreateEventFn;
        public char* m_pNetworkName;
        public RecvTable* m_pRecvTable;
        public ClientClass* m_pNext;
        int m_ClassID;
    };
}
