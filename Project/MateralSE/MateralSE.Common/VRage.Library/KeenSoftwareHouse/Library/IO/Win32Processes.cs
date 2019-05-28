namespace KeenSoftwareHouse.Library.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;
    using System.Threading;
    using Unsharper;

    [UnsharperDisableReflection]
    public class Win32Processes
    {
        private const int CNST_SYSTEM_HANDLE_INFORMATION = 0x10;

        private static string GetFilePath(Win32API.SYSTEM_HANDLE_INFORMATION systemHandleInformation, Process process)
        {
            IntPtr buffer;
            IntPtr ptr3;
            string str4;
            Win32API.OBJECT_BASIC_INFORMATION structure = new Win32API.OBJECT_BASIC_INFORMATION();
            Win32API.OBJECT_TYPE_INFORMATION object_type_information = new Win32API.OBJECT_TYPE_INFORMATION();
            Win32API.OBJECT_NAME_INFORMATION object_name_information = new Win32API.OBJECT_NAME_INFORMATION();
            string strRawName = "";
            int returnLength = 0;
            if (!Win32API.DuplicateHandle(Win32API.OpenProcess(Win32API.ProcessAccessFlags.All, false, process.Id), systemHandleInformation.Handle, Win32API.GetCurrentProcess(), out ptr3, 0, false, 2))
            {
                return null;
            }
            IntPtr objectInformation = Marshal.AllocHGlobal(Marshal.SizeOf<Win32API.OBJECT_BASIC_INFORMATION>(structure));
            Win32API.NtQueryObject(ptr3, 0, objectInformation, Marshal.SizeOf<Win32API.OBJECT_BASIC_INFORMATION>(structure), ref returnLength);
            structure = (Win32API.OBJECT_BASIC_INFORMATION) Marshal.PtrToStructure(objectInformation, structure.GetType());
            Marshal.FreeHGlobal(objectInformation);
            IntPtr ptr5 = Marshal.AllocHGlobal(structure.TypeInformationLength);
            returnLength = structure.TypeInformationLength;
            while (Win32API.NtQueryObject(ptr3, 2, ptr5, returnLength, ref returnLength) == -1073741820)
            {
                if (returnLength == 0)
                {
                    Console.WriteLine("nLength returned at zero! ");
                    return null;
                }
                Marshal.FreeHGlobal(ptr5);
                ptr5 = Marshal.AllocHGlobal(returnLength);
            }
            object_type_information = (Win32API.OBJECT_TYPE_INFORMATION) Marshal.PtrToStructure(ptr5, object_type_information.GetType());
            if (Is64Bits())
            {
                buffer = new IntPtr(Convert.ToInt64(object_type_information.Name.Buffer.ToString(), 10) >> 0x20);
            }
            else
            {
                buffer = object_type_information.Name.Buffer;
            }
            string str2 = Marshal.PtrToStringUni(buffer, object_type_information.Name.Length >> 1);
            Marshal.FreeHGlobal(ptr5);
            if (str2 != "File")
            {
                return null;
            }
            returnLength = structure.NameInformationLength;
            IntPtr ptr6 = Marshal.AllocHGlobal(returnLength);
            while (true)
            {
                if (Win32API.NtQueryObject(ptr3, 1, ptr6, returnLength, ref returnLength) != -1073741820)
                {
                    object_name_information = (Win32API.OBJECT_NAME_INFORMATION) Marshal.PtrToStructure(ptr6, object_name_information.GetType());
                    if (Is64Bits())
                    {
                        buffer = new IntPtr(Convert.ToInt64(object_name_information.Name.Buffer.ToString(), 10) >> 0x20);
                    }
                    else
                    {
                        buffer = object_name_information.Name.Buffer;
                    }
                    if (buffer != IntPtr.Zero)
                    {
                        byte[] destination = new byte[returnLength];
                        try
                        {
                            Marshal.Copy(buffer, destination, 0, returnLength);
                            strRawName = Marshal.PtrToStringUni(Is64Bits() ? new IntPtr(buffer.ToInt64()) : new IntPtr(buffer.ToInt32()));
                        }
                        catch (AccessViolationException)
                        {
                            str4 = null;
                            break;
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(ptr6);
                            Win32API.CloseHandle(ptr3);
                        }
                    }
                    string regularFileNameFromDevice = GetRegularFileNameFromDevice(strRawName);
                    try
                    {
                        str4 = regularFileNameFromDevice;
                    }
                    catch
                    {
                        str4 = null;
                    }
                    break;
                }
                Marshal.FreeHGlobal(ptr6);
                if (returnLength == 0)
                {
                    Console.WriteLine("nLength returned at zero! " + str2);
                    return null;
                }
                ptr6 = Marshal.AllocHGlobal(returnLength);
            }
            return str4;
        }

        public static List<string> GetFilesLockedBy(Process process)
        {
            List<string> outp = new List<string>();
            ThreadStart start = delegate {
                try
                {
                    outp = UnsafeGetFilesLockedBy(process);
                }
                catch
                {
                    Ignore();
                }
            };
            try
            {
                Thread thread = new Thread(start) {
                    IsBackground = true
                };
                thread.Start();
                if (!thread.Join(250))
                {
                    try
                    {
                        thread.Interrupt();
                        thread.Abort();
                    }
                    catch
                    {
                        Ignore();
                    }
                }
            }
            catch
            {
                Ignore();
            }
            return outp;
        }

        private static unsafe IEnumerable<Win32API.SYSTEM_HANDLE_INFORMATION> GetHandles(Process process)
        {
            IntPtr ptr2;
            long num3;
            int cb = 0x10000;
            IntPtr systemInformation = Marshal.AllocHGlobal(cb);
            int returnLength = 0;
            while (Win32API.NtQuerySystemInformation(0x10, systemInformation, cb, ref returnLength) == 0xc0000004)
            {
                cb = returnLength;
                Marshal.FreeHGlobal(systemInformation);
                systemInformation = Marshal.AllocHGlobal(returnLength);
            }
            Marshal.Copy(systemInformation, new byte[returnLength], 0, returnLength);
            if (Is64Bits())
            {
                num3 = Marshal.ReadInt64(systemInformation);
                ptr2 = new IntPtr(systemInformation.ToInt64() + 8L);
            }
            else
            {
                num3 = Marshal.ReadInt32(systemInformation);
                ptr2 = new IntPtr(systemInformation.ToInt32() + 4);
            }
            List<Win32API.SYSTEM_HANDLE_INFORMATION> list = new List<Win32API.SYSTEM_HANDLE_INFORMATION>();
            for (long i = 0L; i < num3; i += 1L)
            {
                Win32API.SYSTEM_HANDLE_INFORMATION structure = new Win32API.SYSTEM_HANDLE_INFORMATION();
                if (Is64Bits())
                {
                    structure = (Win32API.SYSTEM_HANDLE_INFORMATION) Marshal.PtrToStructure(ptr2, structure.GetType());
                    IntPtr* ptrPtr1 = (IntPtr*) ref ptr2;
                    ptrPtr1 = (IntPtr*) new IntPtr((ptr2.ToInt64() + Marshal.SizeOf<Win32API.SYSTEM_HANDLE_INFORMATION>(structure)) + 8L);
                }
                else
                {
                    IntPtr* ptrPtr2 = (IntPtr*) ref ptr2;
                    ptrPtr2 = (IntPtr*) new IntPtr(ptr2.ToInt64() + Marshal.SizeOf<Win32API.SYSTEM_HANDLE_INFORMATION>(structure));
                    structure = (Win32API.SYSTEM_HANDLE_INFORMATION) Marshal.PtrToStructure(ptr2, structure.GetType());
                }
                if (structure.ProcessID == process.Id)
                {
                    list.Add(structure);
                }
            }
            return list;
        }

        public static List<Process> GetProcessesLockingFile(string filePath)
        {
            List<Process> list = new List<Process>();
            foreach (Process process in Process.GetProcesses())
            {
                if ((process.Id > 4) && GetFilesLockedBy(process).Contains(filePath))
                {
                    list.Add(process);
                }
            }
            return list;
        }

        private static string GetRegularFileNameFromDevice(string strRawName)
        {
            string str = strRawName;
            string[] logicalDrives = Environment.GetLogicalDrives();
            int index = 0;
            while (true)
            {
                if (index < logicalDrives.Length)
                {
                    string str2 = logicalDrives[index];
                    StringBuilder lpTargetPath = new StringBuilder(260);
                    if (Win32API.QueryDosDevice(str2.Substring(0, 2), lpTargetPath, 260) == 0)
                    {
                        return strRawName;
                    }
                    string str3 = lpTargetPath.ToString();
                    if (!str.StartsWith(str3))
                    {
                        index++;
                        continue;
                    }
                    str = str.Replace(str3, str2.Substring(0, 2));
                }
                return str;
            }
        }

        private static void Ignore()
        {
        }

        private static bool Is64Bits() => 
            (Marshal.SizeOf(typeof(IntPtr)) == 8);

        private static List<string> UnsafeGetFilesLockedBy(Process process)
        {
            try
            {
                List<string> list = new List<string>();
                foreach (Win32API.SYSTEM_HANDLE_INFORMATION system_handle_information in GetHandles(process))
                {
                    string filePath = GetFilePath(system_handle_information, process);
                    if (filePath != null)
                    {
                        list.Add(filePath);
                    }
                }
                return list;
            }
            catch
            {
                return new List<string>();
            }
        }

        internal class Win32API
        {
            public const int MAX_PATH = 260;
            public const uint STATUS_INFO_LENGTH_MISMATCH = 0xc0000004;
            public const int DUPLICATE_SAME_ACCESS = 2;
            public const uint FILE_SEQUENTIAL_ONLY = 4;

            [DllImport("kernel32.dll")]
            public static extern int CloseHandle(IntPtr hObject);
            [return: MarshalAs(UnmanagedType.Bool)]
            [DllImport("kernel32.dll", SetLastError=true)]
            public static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, ushort hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);
            [DllImport("kernel32.dll")]
            public static extern IntPtr GetCurrentProcess();
            [DllImport("ntdll.dll")]
            public static extern int NtQueryObject(IntPtr ObjectHandle, int ObjectInformationClass, IntPtr ObjectInformation, int ObjectInformationLength, ref int returnLength);
            [DllImport("ntdll.dll")]
            public static extern uint NtQuerySystemInformation(int SystemInformationClass, IntPtr SystemInformation, int SystemInformationLength, ref int returnLength);
            [DllImport("kernel32.dll")]
            public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);
            [DllImport("kernel32.dll", SetLastError=true)]
            public static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);

            [StructLayout(LayoutKind.Sequential), UnsharperDisableReflection]
            public struct GENERIC_MAPPING
            {
                public int GenericRead;
                public int GenericWrite;
                public int GenericExecute;
                public int GenericAll;
            }

            [StructLayout(LayoutKind.Sequential), UnsharperDisableReflection]
            public struct OBJECT_BASIC_INFORMATION
            {
                public int Attributes;
                public int GrantedAccess;
                public int HandleCount;
                public int PointerCount;
                public int PagedPoolUsage;
                public int NonPagedPoolUsage;
                public int Reserved1;
                public int Reserved2;
                public int Reserved3;
                public int NameInformationLength;
                public int TypeInformationLength;
                public int SecurityDescriptorLength;
                public System.Runtime.InteropServices.ComTypes.FILETIME CreateTime;
            }

            [StructLayout(LayoutKind.Sequential), UnsharperDisableReflection]
            public struct OBJECT_NAME_INFORMATION
            {
                public Win32Processes.Win32API.UNICODE_STRING Name;
            }

            [StructLayout(LayoutKind.Sequential), UnsharperDisableReflection]
            public struct OBJECT_TYPE_INFORMATION
            {
                public Win32Processes.Win32API.UNICODE_STRING Name;
                public int ObjectCount;
                public int HandleCount;
                public int Reserved1;
                public int Reserved2;
                public int Reserved3;
                public int Reserved4;
                public int PeakObjectCount;
                public int PeakHandleCount;
                public int Reserved5;
                public int Reserved6;
                public int Reserved7;
                public int Reserved8;
                public int InvalidAttributes;
                public Win32Processes.Win32API.GENERIC_MAPPING GenericMapping;
                public int ValidAccess;
                public byte Unknown;
                public byte MaintainHandleDatabase;
                public int PoolType;
                public int PagedPoolUsage;
                public int NonPagedPoolUsage;
            }

            public enum ObjectInformationClass
            {
                ObjectBasicInformation,
                ObjectNameInformation,
                ObjectTypeInformation,
                ObjectAllTypesInformation,
                ObjectHandleInformation
            }

            [Flags]
            public enum ProcessAccessFlags : uint
            {
                All = 0x1f0fff,
                Terminate = 1,
                CreateThread = 2,
                VMOperation = 8,
                VMRead = 0x10,
                VMWrite = 0x20,
                DupHandle = 0x40,
                SetInformation = 0x200,
                QueryInformation = 0x400,
                Synchronize = 0x100000
            }

            [StructLayout(LayoutKind.Sequential, Pack=1)]
            public struct SYSTEM_HANDLE_INFORMATION
            {
                public int ProcessID;
                public byte ObjectTypeNumber;
                public byte Flags;
                public ushort Handle;
                public int Object_Pointer;
                public uint GrantedAccess;
            }

            [StructLayout(LayoutKind.Sequential, Pack=1), UnsharperDisableReflection]
            public struct UNICODE_STRING
            {
                public ushort Length;
                public ushort MaximumLength;
                public IntPtr Buffer;
            }
        }
    }
}

