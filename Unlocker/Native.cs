using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Unlocker
{
    public class Win32API
    {
        [DllImport("advapi32.dll", ExactSpelling = true)]
        public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, int BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

        [DllImport("kernel32.dll")]
        public static extern bool IsWow64Process(IntPtr hProcess, out bool Wow64Process);

        [DllImport("ntdll.dll")]
        public static extern uint NtQueryObject(IntPtr ObjectHandle, int
            ObjectInformationClass, IntPtr ObjectInformation, int ObjectInformationLength,
            ref int returnLength);

        [DllImport("ntdll.dll")]
        public static extern uint NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS
            SystemInformationClass, IntPtr SystemInformation, int SystemInformationLength,
            ref int returnLength);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, UInt32 dwProcessId);

        [DllImport("advapi32.dll", ExactSpelling = true)]
        public static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DuplicateHandle(IntPtr hSourceProcessHandle,
           ushort hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle,
           uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, DuplicateOptions dwOptions);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref long lpLuid);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);

        [DllImport("User32.dll")]
        public static extern int DestroyIcon(IntPtr hIcon);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [Flags]
        public enum DuplicateOptions : uint
        {
            DUPLICATE_CLOSE_SOURCE = 0x00000001,
            DUPLICATE_SAME_ACCESS = 0x00000002
        }

        public enum ObjectInformationClass : int
        {
            ObjectBasicInformation = 0,
            ObjectNameInformation = 1,
            ObjectTypeInformation = 2,
            ObjectAllTypesInformation = 3,
            ObjectHandleInformation = 4
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }
        
        [Flags]
        public enum SYSTEM_INFORMATION_CLASS : uint
        {
            SystemHandleInformation = 16
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_TYPE_INFORMATION
        {
            // Information Class 2
            public UNICODE_STRING Name;
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
            public GENERIC_MAPPING GenericMapping;
            public int ValidAccess;
            public byte Unknown;
            public byte MaintainHandleDatabase;
            public int PoolType;
            public int PagedPoolUsage;
            public int NonPagedPoolUsage;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OBJECT_NAME_INFORMATION
        {
            // Information Class 1
            public UNICODE_STRING Name;
        }

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct UNICODE_STRING
        {
            internal UInt16 Length;
            internal UInt16 MaximumLength;
            internal string Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GENERIC_MAPPING
        {
            public int GenericRead;
            public int GenericWrite;
            public int GenericExecute;
            public int GenericAll;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SYSTEM_HANDLE_INFORMATION
        {
            // Information Class 16
            public UInt16 UniqueProcessId;
            public UInt16 CreatorBackTraceIndex;
            public Byte ObjectTypeIndex;
            public Byte HandleAttributes; // 0x01 = PROTECT_FROM_CLOSE, 0x02 = INHERIT
            public UInt16 HandleValue;
            public IntPtr Object;
            public IntPtr GrantedAccess;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public long Luid;
            public int Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public const int NAMESIZE = 80;
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NAMESIZE)]
            public string szTypeName;
        };

        public const uint SHGFI_ICON = 0x000000100;
        public const uint SHGFI_LINKOVERLAY = 0x000008000;
        public const uint SHGFI_LARGEICON = 0x000000000;
        public const uint SHGFI_SMALLICON = 0x000000001;
        public const uint SHGFI_OPENICON = 0x000000002;
        public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

        public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

        public const int MAX_PATH = 260;

        public const int SE_PRIVILEGE_ENABLED = 0x00000002;
        public const int TOKEN_QUERY = 0x00000008;
        public const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        public const string SE_DEBUG_NAME = "SeDebugPrivilege";

        public const uint STATUS_INFO_LENGTH_MISMATCH = 0xc0000004;
        public const uint STATUS_BUFFER_OVERFLOW = 0x80000005;
    }

    public class Win32Process
    {
        public static bool EnablePrivilege(string strPriName, bool bEnable)
        {
            bool bRet = false;
            Win32API.TOKEN_PRIVILEGES tp;
            IntPtr hToken = IntPtr.Zero;

            bRet = Win32API.OpenProcessToken(Win32API.GetCurrentProcess(), Win32API.TOKEN_ADJUST_PRIVILEGES | Win32API.TOKEN_QUERY, ref hToken);

            tp.PrivilegeCount = 1;
            tp.Luid = 0;

            if (bEnable)
                tp.Attributes = Win32API.SE_PRIVILEGE_ENABLED;
            else
                tp.Attributes = 0;

            bRet = Win32API.LookupPrivilegeValue(null, Win32API.SE_DEBUG_NAME, ref tp.Luid);
            bRet = Win32API.AdjustTokenPrivileges(hToken, false, ref tp, Marshal.SizeOf(tp), IntPtr.Zero, IntPtr.Zero);

            return bRet;
        }

        public static string GetDosPath(string strDevName)
        {
            string strFileName = strDevName;

            foreach (string strDrivePath in Environment.GetLogicalDrives())
            {
                StringBuilder sb = new StringBuilder(Win32API.MAX_PATH);

                if (Win32API.QueryDosDevice(strDrivePath.Substring(0, 2), sb, Win32API.MAX_PATH) == 0)
                    return strDevName;

                string strTargetPath = sb.ToString();

                if (strFileName.StartsWith(strTargetPath))
                {
                    strFileName = strFileName.Replace(strTargetPath, strDrivePath.Substring(0, 2));
                    break;
                }
            }

            return strFileName;
        }

        public static string GetObjectName(IntPtr ipProcess, Win32API.SYSTEM_HANDLE_INFORMATION shHandle)
        {
            try
            {
                IntPtr ipHandle = IntPtr.Zero;
                var objObjectName = new Win32API.OBJECT_NAME_INFORMATION();
                IntPtr ipObjectName = IntPtr.Zero;
                int nLength = 0;
                uint nReturn = 0;

                if (!Win32API.DuplicateHandle(ipProcess, shHandle.HandleValue,Win32API.GetCurrentProcess(), out ipHandle,0, false, Win32API.DuplicateOptions.DUPLICATE_SAME_ACCESS))
                    return null;

                nLength = 0x200;
                ipObjectName = Marshal.AllocHGlobal(nLength);

                do
                {
                    nReturn = Win32API.NtQueryObject(ipHandle, (int)Win32API.ObjectInformationClass.ObjectNameInformation,ipObjectName, nLength, ref nLength);
                    if (nReturn == Win32API.STATUS_INFO_LENGTH_MISMATCH)
                    {
                        Marshal.FreeHGlobal(ipObjectName);
                        ipObjectName = Marshal.AllocHGlobal(nLength);
                    }
                    else
                        break;
                }
                while (true);

                objObjectName = (Win32API.OBJECT_NAME_INFORMATION)Marshal.PtrToStructure(ipObjectName, objObjectName.GetType());

                Win32API.CloseHandle(ipHandle);
                Marshal.FreeHGlobal(ipObjectName);

                return objObjectName.Name.Buffer;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string GetObjectName(IntPtr ipProcess, Win32API.SYSTEM_HANDLE_INFORMATION shHandle, TimeSpan timeout)
        {
            string name = null;

            AutoResetEvent signal = new AutoResetEvent(false);
            Thread workerThread = null;

            ThreadPool.QueueUserWorkItem((o) =>
            {
                workerThread = Thread.CurrentThread;
                name = GetObjectName(ipProcess, shHandle);
                signal.Set();
            });

            bool waitres = signal.WaitOne(timeout);

            if (workerThread != null && workerThread.IsAlive && waitres == false)
            {
                workerThread.Abort();
            }

            return name;
        }

        public static string GetObjectTypeName(IntPtr ipProcess, Win32API.SYSTEM_HANDLE_INFORMATION shHandle)
        {
            try
            {
                IntPtr ipHandle = IntPtr.Zero;
                var objObjectType = new Win32API.OBJECT_TYPE_INFORMATION();
                IntPtr ipObjectType = IntPtr.Zero;
                int nLength = 0;
                uint nReturn = 0;

                if (!Win32API.DuplicateHandle(ipProcess, shHandle.HandleValue, Win32API.GetCurrentProcess(), out ipHandle, 0, false, Win32API.DuplicateOptions.DUPLICATE_SAME_ACCESS))
                    return null;

                nLength = 0x200;
                ipObjectType = Marshal.AllocHGlobal(nLength);

                do
                {
                    nReturn = Win32API.NtQueryObject(ipHandle, (int)Win32API.ObjectInformationClass.ObjectTypeInformation, ipObjectType, nLength, ref nLength);
                    if (nReturn == Win32API.STATUS_INFO_LENGTH_MISMATCH)
                    {
                        Marshal.FreeHGlobal(ipObjectType);
                        ipObjectType = Marshal.AllocHGlobal(nLength);
                    }
                    else
                        break;
                }
                while (true);

                objObjectType = (Win32API.OBJECT_TYPE_INFORMATION)Marshal.PtrToStructure(ipObjectType, objObjectType.GetType());

                Marshal.FreeHGlobal(ipObjectType);

                Win32API.CloseHandle(ipHandle);

                return objObjectType.Name.Buffer;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<Win32API.SYSTEM_HANDLE_INFORMATION> GetHandles()
        {
            uint nStatus;
            int nHandleInfoSize = 0x10000;
            IntPtr ipHandlePointer = Marshal.AllocHGlobal(nHandleInfoSize);
            int nLength = 0;
            long lHandleCount = 0;
            IntPtr ipHandle = IntPtr.Zero;

            Win32API.SYSTEM_HANDLE_INFORMATION shHandle;
            List<Win32API.SYSTEM_HANDLE_INFORMATION> lstHandles = new List<Win32API.SYSTEM_HANDLE_INFORMATION>();

            try
            {
                while ((nStatus = Win32API.NtQuerySystemInformation(Win32API.SYSTEM_INFORMATION_CLASS.SystemHandleInformation, ipHandlePointer, nHandleInfoSize, ref nLength)) == Win32API.STATUS_INFO_LENGTH_MISMATCH)
                {
                    nHandleInfoSize = nLength;
                    Marshal.FreeHGlobal(ipHandlePointer);
                    ipHandlePointer = Marshal.AllocHGlobal(nLength);
                }

                lHandleCount = Marshal.ReadIntPtr(ipHandlePointer).ToInt64();
                ipHandle = new IntPtr(ipHandlePointer.ToInt64() + Marshal.SizeOf(ipHandlePointer));

                for (long lIndex = 0; lIndex < lHandleCount; lIndex++)
                {
                    shHandle = (Win32API.SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(ipHandle, typeof(Win32API.SYSTEM_HANDLE_INFORMATION));
                    ipHandle = new IntPtr(ipHandle.ToInt64() + Marshal.SizeOf(new Win32API.SYSTEM_HANDLE_INFORMATION()));

                    lstHandles.Add(shHandle);
                }

                Marshal.FreeHGlobal(ipHandlePointer);
            }
            catch (Exception ex)
            {
                
            }

            return lstHandles;
        }

        public static void CloseHandleEx(UInt32 pid, ushort handle)
        {
            IntPtr hRemoteProcess = IntPtr.Zero;
            IntPtr hDupHandle = IntPtr.Zero;

            hRemoteProcess = Win32API.OpenProcess(Win32API.ProcessAccessFlags.DupHandle, false, pid);
            if (hRemoteProcess == IntPtr.Zero)
                return;

            if (!Win32API.DuplicateHandle(hRemoteProcess, handle, Win32API.GetCurrentProcess(), out hDupHandle, 0, false, Win32API.DuplicateOptions.DUPLICATE_CLOSE_SOURCE | Win32API.DuplicateOptions.DUPLICATE_SAME_ACCESS))
                return;

            Win32API.CloseHandle(hDupHandle);
            Win32API.CloseHandle(hRemoteProcess);
        }
    }

    public class Win32Icon
    {
        private static bool IsLogicalDrive(string path)
        {
            bool bRet = false;
            string drive = path.ToLower();

            if (path.Length >= 2 && path.Contains(":"))
            {
                string[] drives = Directory.GetLogicalDrives();
                foreach (string d in drives)
                {
                    if (d.ToLower().Contains(drive))
                    {
                        bRet = true;
                        break;
                    }
                }
            }

            return bRet;
        }

        public static Icon GetFileIcon(string fileName, bool smallIcon)
        {
            var sfi = new Win32API.SHFILEINFO();
            Icon icon = null;

            int nTotal = (int)Win32API.SHGetFileInfo(fileName, 100, ref sfi, 0, (uint)(smallIcon ? 273 : 272));
            if (nTotal > 0)
                icon = (Icon)Icon.FromHandle(sfi.hIcon).Clone();

            if (sfi.hIcon != IntPtr.Zero)
                Win32API.DestroyIcon(sfi.hIcon);

            return icon;
        }

        public static Icon GetFolderIcon(Boolean largeIcon, Boolean openFolder)
        {
            var sfi = new Win32API.SHFILEINFO();
            Icon icon = null;

            uint flags = Win32API.SHGFI_ICON | Win32API.SHGFI_USEFILEATTRIBUTES;
            flags |= openFolder ? Win32API.SHGFI_OPENICON : 0;
            flags |= largeIcon ? Win32API.SHGFI_LARGEICON : Win32API.SHGFI_SMALLICON;

            string strWinDir = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).FullName;

            int nTotal = (int)Win32API.SHGetFileInfo(strWinDir, Win32API.FILE_ATTRIBUTE_DIRECTORY, ref sfi, (uint)Marshal.SizeOf(sfi), flags);
            if (nTotal > 0)
                icon = (Icon)Icon.FromHandle(sfi.hIcon).Clone();

            if (sfi.hIcon != IntPtr.Zero)
                Win32API.DestroyIcon(sfi.hIcon);

            return icon;
        }

        public static Icon GetFileIconEx(string fileName, bool smallIcon)
        {
            Icon icon = null;

            if (!IsLogicalDrive(fileName) && Directory.Exists(fileName))
                icon = GetFolderIcon(!smallIcon, false);
            else
                icon = GetFileIcon(fileName, smallIcon);

            return icon;
        }
    }
}