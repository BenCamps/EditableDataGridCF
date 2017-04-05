using System;
using System.Collections.Generic;
using System.Text;

namespace EditableDataGridCF
{
    public delegate void EditableDataGridCellValidatingEventHandler(Object sender, EditableDataGridCellValidatingEventArgs e);
    public delegate void EditableDataGridCellValueChangedEventHandler(Object sender, EditableDataGridCellEventArgs e);

    public delegate void ButtonCellClickEventHandler(ButtonCellClickEventArgs e);
}
