using System;
using System.Windows.Forms;

namespace EditableDataGridCF
{
    public class EditableDataGridCellEventArgs
    {
        public DataGridColumnStyle Column { get; set; }

        public int RowIndex { get; set; }
    }

    public class EditableDataGridCellValidatingEventArgs
    {
        public bool Cancel { get; set; }

        public DataGridColumnStyle Column { get; set; }

        public int RowIndex { get; set; }

        public object Value { get; set; }
    }

    public class ButtonCellClickEventArgs
    {
        public object CellValue { get; set; }

        public DataGrid DataGrid { get; set; }

        public int RowNumber { get; set; }

    }
}
