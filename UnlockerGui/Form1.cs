using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Unlocker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string _lastFilePath = "";

        private void EnableWindow(bool enable)
        {
            btnRefresh.Enabled = enable;
            btnBrowseFile.Enabled = enable;
            btnBrowseDir.Enabled = enable;
            btnKillProcess.Enabled = enable;
            btnUnlock.Enabled = enable;
            btnUnlockAll.Enabled = enable;
        }

        private void GetLockedFiles(string path)
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    EnableWindow(false);

                    listView1.Items.Clear();
                    listView1.BeginUpdate();
                    imageList1.Images.Clear();

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

                                ListViewItem item = new ListViewItem(strProName);

                                var icon = Win32Icon.GetFileIconEx(pro.MainModule.FileName, true);
                                if(icon != null)
                                {
                                    imageList1.Images.Add(icon);
                                    item.ImageIndex = imageList1.Images.Count - 1;
                                }

                                item.Tag = handle;
                                item.SubItems.AddRange(new string[] { handle.HandleValue.ToString("x8"), file, handle.UniqueProcessId.ToString(), pro.MainModule.FileName });
                                listView1.Items.Add(item);
                            }
                        }

                        Win32API.CloseHandle(ipProcess);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    _lastFilePath = path;
                    listView1.EndUpdate();
                    EnableWindow(true);
                }
            });
            thread.IsBackground = true;
            thread.Start();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Win32Process.EnablePrivilege(Win32API.SE_DEBUG_NAME, true);
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewEx lv = sender as ListViewEx;

            ListViewItemComparer sorter = (ListViewItemComparer)lv.ListViewItemSorter;
            if (sorter == null)
                sorter = new ListViewItemComparer();

            sorter.ColumnIndex = e.Column;
            switch (e.Column)
            {
                case 3:
                    sorter.ColumnType = ColumnDataType.Int;
                    break;
                default:
                    sorter.ColumnType = ColumnDataType.String;
                    break;
            }

            if (sorter.SortDirection == SortOrder.Ascending)
                sorter.SortDirection = SortOrder.Descending;
            else
                sorter.SortDirection = SortOrder.Ascending;

            lv.ListViewItemSorter = sorter;
            lv.Sort();

            for (int i = 0; i < lv.Columns.Count; i++)
            {
                lv.SetColumnHeaderSortIcon(i, i == sorter.ColumnIndex ? sorter.SortDirection : SortOrder.None);
            }
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])(e.Data.GetData(DataFormats.FileDrop));
                GetLockedFiles(paths[0]);
            }
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            GetLockedFiles(_lastFilePath);
        }

        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.ValidateNames = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                GetLockedFiles(ofd.FileName);
            }
        }

        private void btnBrowseDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                GetLockedFiles(fbd.SelectedPath);
            }
        }

        private void btnKillProcess_Click(object sender, EventArgs e)
        {
            EnableWindow(false);

            for (int i = listView1.Items.Count - 1; i >= 0; i--)
            {
                var item = listView1.Items[i];
                if (item.Selected)
                {
                    try
                    {
                        var pid = ((Win32API.SYSTEM_HANDLE_INFORMATION)item.Tag).UniqueProcessId;
                        Process.GetProcessById((int)pid).Kill();
                    }
                    catch (Exception ex) { }
                }
            }

            EnableWindow(true);
        }

        private void btnUnlock_Click(object sender, EventArgs e)
        {
            EnableWindow(false);

            for(int i = listView1.Items.Count - 1; i >= 0; i--)
            {
                var item = listView1.Items[i];
                if (item.Selected)
                {
                    var shi = (Win32API.SYSTEM_HANDLE_INFORMATION)item.Tag;
                    Win32Process.CloseHandleEx(shi.UniqueProcessId, shi.HandleValue);
                    item.Remove();
                }
            }

            EnableWindow(true);
        }

        private void btnUnlockAll_Click(object sender, EventArgs e)
        {
            EnableWindow(false);

            for (int i = listView1.Items.Count - 1; i >= 0; i--)
            {
                var item = listView1.Items[i];
                var shi = (Win32API.SYSTEM_HANDLE_INFORMATION)item.Tag;
                Win32Process.CloseHandleEx(shi.UniqueProcessId, shi.HandleValue);
                item.Remove();
            }

            EnableWindow(true);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}