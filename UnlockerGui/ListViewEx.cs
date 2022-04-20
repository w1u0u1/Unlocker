using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Unlocker
{
    public class ListViewEx : ListView
    {
        private const int LVM_FIRST = 0x1000;
        private const int LVM_SETITEMSTATE = LVM_FIRST + 43;

        // Parameters for ListView-Headers
        private const Int32 HDI_FORMAT = 0x0004;
        private const Int32 HDF_LEFT = 0x0000;
        private const Int32 HDF_STRING = 0x4000;
        private const Int32 HDF_SORTUP = 0x0400;
        private const Int32 HDF_SORTDOWN = 0x0200;
        private const Int32 LVM_GETHEADER = 0x1000 + 31;  // LVM_FIRST + 31
        private const Int32 HDM_GETITEM = 0x1200 + 11;  // HDM_FIRST + 11
        private const Int32 HDM_SETITEM = 0x1200 + 12;  // HDM_FIRST + 12

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct LVITEM
        {
            public int mask;
            public int iItem;
            public int iSubItem;
            public int state;
            public int stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
            public int iIndent;
            public int iGroupId;
            public int cColumns;
            public IntPtr puColumns;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct HDITEM
        {
            public Int32 mask;
            public Int32 cxy;
            [MarshalAs(UnmanagedType.LPTStr)]
            public String pszText;
            public IntPtr hbm;
            public Int32 cchTextMax;
            public Int32 fmt;
            public Int32 lParam;
            public Int32 iImage;
            public Int32 iOrder;
        };

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern IntPtr SendMessageHDItem(IntPtr Handle, Int32 msg, IntPtr wParam, ref HDITEM lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageLVItem(IntPtr hWnd, int msg, int wParam, ref LVITEM lvi);

        public ListViewEx()
        {
            SetStyle(ControlStyles.DoubleBuffer |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0014)
                return;
            base.WndProc(ref m);
        }

        private void SetItemState(ListView list, int itemIndex, int mask, int value)
        {
            LVITEM lvItem = new LVITEM();
            lvItem.stateMask = mask;
            lvItem.state = value;
            SendMessageLVItem(list.Handle, LVM_SETITEMSTATE, itemIndex, ref lvItem);
        }

        public void SetColumnHeaderSortIcon(int colIndex, SortOrder order)
        {
            IntPtr hColHeader = SendMessage(this.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);

            HDITEM hdItem = new HDITEM();
            IntPtr colHeader = new IntPtr(colIndex);

            hdItem.mask = HDI_FORMAT;
            IntPtr rtn = SendMessageHDItem(hColHeader, HDM_GETITEM, colHeader, ref hdItem);

            if (order == SortOrder.Ascending)
            {
                hdItem.fmt &= ~HDF_SORTDOWN;
                hdItem.fmt |= HDF_SORTUP;
            }
            else if (order == SortOrder.Descending)
            {
                hdItem.fmt &= ~HDF_SORTUP;
                hdItem.fmt |= HDF_SORTDOWN;
            }
            else if (order == SortOrder.None)
            {
                hdItem.fmt &= ~HDF_SORTDOWN & ~HDF_SORTUP;
            }

            rtn = SendMessageHDItem(hColHeader, HDM_SETITEM, colHeader, ref hdItem);
        }

        public void SelectAllItems()
        {
            SetItemState(this, -1, 2, 2);
        }

        public void DeselectAllItems()
        {
            SetItemState(this, -1, 2, 0);
        }

        public void Export()
        {
            if (Items.Count == 0)
                return;

            SaveFileDialog SFD = new SaveFileDialog();
            SFD.Filter = "Text Documents(*.txt)|*.txt|All Files(*.*)|*.*";

            if (SFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    StreamWriter sw = new StreamWriter(SFD.FileName);
                    ColumnHeaderCollection columns = Columns;

                    foreach (ColumnHeader col in columns)
                        sw.Write(col.Text + "\t");

                    sw.Write("\r\n");

                    foreach (ListViewItem item in Items)
                    {
                        for (int i = 0; i < columns.Count; i++)
                        {
                            try
                            {
                                sw.Write(item.SubItems[i].Text + "\t");
                            }
                            catch (Exception ex) { }
                        }
                        sw.Write("\r\n");
                    }

                    sw.Close();
                }
                catch (Exception ex)
                {

                }
            }
        }
    }

    enum ColumnDataType
    {
        DateTime,
        Decimal,
        Short,
        Int,
        Long,
        String
    }

    class ListViewItemComparer : IComparer
    {
        private int _columnIndex;
        public int ColumnIndex
        {
            get
            {
                return _columnIndex;
            }
            set
            {
                _columnIndex = value;
            }
        }

        private SortOrder _sortDirection;
        public SortOrder SortDirection
        {
            get
            {
                return _sortDirection;
            }
            set
            {
                _sortDirection = value;
            }
        }

        private ColumnDataType _columnType;
        public ColumnDataType ColumnType
        {
            get
            {
                return _columnType;
            }
            set
            {
                _columnType = value;
            }
        }

        public ListViewItemComparer()
        {
            _sortDirection = SortOrder.None;
        }

        public int Compare(object x, object y)
        {
            ListViewItem lviX = x as ListViewItem;
            ListViewItem lviY = y as ListViewItem;

            int result;

            if (lviX == null && lviY == null)
                result = 0;
            else if (lviX == null)
                result = -1;
            else if (lviY == null)
                result = 1;

            switch (ColumnType)
            {
                case ColumnDataType.DateTime:
                    DateTime xDt = DateTime.Parse(lviX.SubItems[ColumnIndex].Text);
                    DateTime yDt = DateTime.Parse(lviY.SubItems[ColumnIndex].Text);
                    result = DateTime.Compare(xDt, yDt);
                    break;
                case ColumnDataType.Decimal:
                    Decimal xD = Convert.ToDecimal(lviX.SubItems[ColumnIndex].Text.Replace("$", string.Empty).Replace(",", string.Empty));
                    Decimal yD = Convert.ToDecimal(lviY.SubItems[ColumnIndex].Text.Replace("$", string.Empty).Replace(",", string.Empty));
                    result = Decimal.Compare(xD, yD);
                    break;
                case ColumnDataType.Short:
                    short xShort = Convert.ToInt16(lviX.SubItems[ColumnIndex].Text);
                    short yShort = Convert.ToInt16(lviY.SubItems[ColumnIndex].Text);
                    result = xShort.CompareTo(yShort);
                    break;
                case ColumnDataType.Int:
                    int xInt = Convert.ToInt32(lviX.SubItems[ColumnIndex].Text);
                    int yInt = Convert.ToInt32(lviY.SubItems[ColumnIndex].Text);
                    result = xInt.CompareTo(yInt);
                    break;
                case ColumnDataType.Long:
                    long xLong = Convert.ToInt64(lviX.SubItems[ColumnIndex].Text);
                    long yLong = Convert.ToInt64(lviY.SubItems[ColumnIndex].Text);
                    result = xLong.CompareTo(yLong);
                    break;
                default:
                    result = string.Compare(lviX.SubItems[ColumnIndex].Text, lviY.SubItems[ColumnIndex].Text, false);
                    break;
            }

            if (SortDirection == SortOrder.Descending)
                return -result;
            else
                return result;
        }
    }
}