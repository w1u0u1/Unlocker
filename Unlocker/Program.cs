using Gnu.Getopt;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Unlocker
{
    class Program
    {
        static void GetLockedFiles(string path)
        {
            try
            {
                string pathLower = path.ToLower();

                var handles = Win32Process.GetHandles();

                var processes = Process.GetProcesses();
                foreach (var pro in processes)
                {
                    if (pro.Id == Process.GetCurrentProcess().Id)
                        continue;

                    IntPtr ipProcess = Win32API.OpenProcess(Win32API.ProcessAccessFlags.All, false, (UInt32)pro.Id);
                    if (ipProcess == IntPtr.Zero)
                        continue;

                    List<Win32API.SYSTEM_HANDLE_INFORMATION> proHandles = new List<Win32API.SYSTEM_HANDLE_INFORMATION>();
                    foreach (var handle in handles)
                    {
                        if (handle.UniqueProcessId == pro.Id)
                        {
                            proHandles.Add(handle);
                        }
                    }

                    foreach (var handle in proHandles)
                    {
                        var strTypeName = Win32Process.GetObjectTypeName(ipProcess, handle);
                        if (string.IsNullOrEmpty(strTypeName))
                            continue;

                        if (strTypeName != "File")
                            continue;

                        var strDosPath = Win32Process.GetObjectName(ipProcess, handle, TimeSpan.FromMilliseconds(500));
                        if (string.IsNullOrEmpty(strDosPath))
                            continue;

                        var file = Win32Process.GetDosPath(strDosPath);

                        if (file.ToLower().Contains(pathLower))
                        {
                            string strProName = pro.MainModule.ModuleName;

                            bool bRet = false;

                            if (Win32API.IsWow64Process(pro.Handle, out bRet))
                            {
                                if (bRet)
                                    strProName = strProName + "(32 bit)";
                            }

                            Console.WriteLine("{0,-10}\t{1,-8}\t{2,-30}\t{3,-64}\t{4}", handle.HandleValue.ToString("x8"), handle.UniqueProcessId.ToString(), strProName, pro.MainModule.FileName, file);
                        }
                    }

                    Win32API.CloseHandle(ipProcess);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void CloseHandle(List<string> handles)
        {
            try
            {
                foreach (var handle in handles)
                {
                    string[] splits = handle.Split(':');
                    if (splits.Length == 2)
                        Win32Process.CloseHandleEx(uint.Parse(splits[1]), ushort.Parse(splits[0], System.Globalization.NumberStyles.HexNumber));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void Main(string[] args)
        {
            string progname = Path.GetFileName(Assembly.GetExecutingAssembly().CodeBase);

            string path = null;
            List<string> handles = new List<string>();

            Getopt opt = new Getopt(progname, args, "l:c");
            int c;
            while ((c = opt.getopt()) != -1)
            {
                switch (c)
                {
                    case 'l':
                        path = opt.Optarg;
                        break;
                    case 'c':
                        handles.Add(opt.Optarg);
                        break;
                    default:
                        return;
                }
            }

            Win32Process.EnablePrivilege(Win32API.SE_DEBUG_NAME, true);

            if (path != null)
                GetLockedFiles(path);

            if (handles.Count > 0)
                CloseHandle(handles);
        }
    }
}