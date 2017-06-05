using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.WindowsCE.Forms;

namespace EditableDataGridCF
{
    public class EditableDataGrid : DataGrid, System.ComponentModel.ISupportInitialize, IKeyPressProcessor
    {
        #region static members

        private static FieldInfo _firstRowVisableAccessor;
        private static FieldInfo _lastRowVisableAccessor;
        private static FieldInfo _gridRecAccessor;
        private static FieldInfo _gridRendererAccessor;
        private static FieldInfo _horzScrollBarAccessor;
        private static FieldInfo _listManagerAccessor;
        private static FieldInfo _rowHeightAccessor;
        private static FieldInfo _tableStyleAccessor;
        private static FieldInfo _vertScrollBarAccessor;

        static EditableDataGrid()
        {
            _firstRowVisableAccessor = typeof(EditableDataGrid).GetField("m_irowVisibleFirst", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            _lastRowVisableAccessor = typeof(EditableDataGrid).GetField("m_irowVisibleLast", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            _horzScrollBarAccessor = typeof(EditableDataGrid).GetField("m_sbHorz", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            _vertScrollBarAccessor = typeof(EditableDataGrid).GetField("m_sbVert", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            _tableStyleAccessor = typeof(EditableDataGrid).GetField("m_tabstyActive", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            _rowHeightAccessor = typeof(EditableDataGrid).GetField("m_cyRow", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            _listManagerAccessor = typeof(EditableDataGrid).GetField("m_cmData", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);

            _gridRendererAccessor = typeof(EditableDataGrid).GetField("m_renderer", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            Type gridRenderer = (_gridRendererAccessor != null) ? _gridRendererAccessor.FieldType : null;
            if (gridRenderer != null)
            {
                _gridRecAccessor = gridRenderer.GetField("m_rcGrid", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            }
        }

        #endregion static members

        Color _alternatingBackColor = Color.LightGray;
        Color _errorColor = Color.Red;
        

        EditableColumnBase _editColumn;
        bool _inCommitEdit;
        bool _isEditing;
        bool _isScrolling;
        bool _readOnly;

        int _homeColumnIndex = 0;
        InputPanel _sip;

        public EditableDataGrid()
            : base()
        {
            //if not in design mode
            if (!this.InDesignMode())
            {
                WireGridEvents();
            }
        }

        public event EditableDataGridCellValidatingEventHandler CellValidating;

        public event EditableDataGridCellValueChangedEventHandler CellValueChanged;

        #region props

        #region Behavior

        public bool AllowUserToAddRows { get; set; }

        public bool ReadOnly
        {
            get { return _readOnly; }
            set
            {
                if (_readOnly == value) { return; }
                _readOnly = value;
                this.OnReadOnlyChanged();
            }
        }

        #endregion Behavior

        #region Appearance

        public Color AlternatingBackColor
        {
            get { return _alternatingBackColor; }
            set
            {
                _alternatingBackColor = value;
                AlternatingBackBrush = null;
            }
        }

        public Color ErrorColor
        {
            get { return _errorColor; }
            set
            {
                _errorColor = value;                
                ErrorBrush = null;
            }
        }

        SolidBrush _alternatingBackBrush;
        internal SolidBrush AlternatingBackBrush 
        {
            get
            {
                if (_alternatingBackBrush == null)
                { AlternatingBackBrush = new SolidBrush(AlternatingBackColor); }
                return _alternatingBackBrush;
            }
            private set
            {
                if (_alternatingBackBrush != null)
                { _alternatingBackBrush.Dispose(); }
                _alternatingBackBrush = value;
            }
        }

        SolidBrush _altTextBrush;        
        private SolidBrush AltTextBrush
        {
            get
            {
                if (_altTextBrush == null)
                {
                    _altTextBrush = new SolidBrush(this.HeaderForeColor);
                }
                return _altTextBrush;
            }
            set
            {
                if (_altTextBrush != null)
                { _altTextBrush.Dispose(); }
                _altTextBrush = value;
            }
        }
        SolidBrush _errorBrush;
        internal SolidBrush ErrorBrush 
        {
            get
            {
                if (_errorBrush == null)
                { ErrorBrush = new SolidBrush(ErrorColor); }
                return _errorBrush;
            }
            private set
            {
                if (_errorBrush != null)
                { _errorBrush.Dispose(); }
                _errorBrush = value;
            }
        }

        Pen _forePen;
        internal Pen ForePen
        {
            get
            {
                if (_forePen == null)
                { ForePen = new Pen(this.ForeColor, 2); }
                return this._forePen;
            }
            private set
            {
                if (_forePen != null)
                { _forePen.Dispose(); }
                _forePen = value;
            }
        }

        #endregion Appearance

        public int ColumnCount
        {
            get
            {
                if (this.TableStyle == null) { return 0; }
                return this.TableStyle.GridColumnStyles.Count;
            }
        }

        public DataGridColumnStyle CurrentCollumn
        {
            get
            {
                DataGridTableStyle ts = this.TableStyle;
                if (ts != null)
                {
                    return ts.GridColumnStyles[this.CurrentColumnIndex];
                }
                else
                {
                    return null;
                }
            }
        }

        public int CurrentColumnIndex
        {
            get
            {
                return this.CurrentCell.ColumnNumber;
            }
            set
            {
                this.CurrentCell = new DataGridCell(this.CurrentCell.RowNumber, value);
            }
        }

        public new object DataSource
        {
            get
            {
                return base.DataSource;
            }
            set
            {
                if (base.DataSource != null && !base.DataSource.Equals(value))
                {
                    if (this.CurrencyManager != null)
                    {
                        this.UnwireDataSource();
                    }
                }
                base.DataSource = value;

                if (this.CurrencyManager != null)
                {
                    this.WireDataSource();
                }
            }
        }

        protected EditableColumnBase EditColumn
        {
            get
            {
                return _editColumn;
            }
            set
            {
                if (value == _editColumn) { return; }
                _editColumn = value;
            }
        }

        public int RowCount
        {
            get
            {
                return this.CurrencyManager.Count;
            }
        }

        #region hacked members

        protected CurrencyManager CurrencyManager { get { return (CurrencyManager)_listManagerAccessor.GetValue(this); } }

        protected int FirstVisibleRow { get { return (int)_firstRowVisableAccessor.GetValue(this); } }

        protected int LastVisibleRow { get { return (int)_lastRowVisableAccessor.GetValue(this); } }

        protected Rectangle GridRec
        {
            get
            {
                if (_gridRecAccessor != null)
                {
                    object render = _gridRendererAccessor.GetValue(this);
                    return (Rectangle)(_gridRecAccessor.GetValue(render) ?? Rectangle.Empty);
                }
                return Rectangle.Empty;
            }
        }

        protected DataGridTableStyle TableStyle { get { return (DataGridTableStyle)_tableStyleAccessor.GetValue(this); } }

        protected ScrollBar HorizScrollBar { get { return (ScrollBar)_horzScrollBarAccessor.GetValue(this); } }

        protected ScrollBar VertScrollBar { get { return (ScrollBar)_vertScrollBarAccessor.GetValue(this); } }

        protected int RowHeight
        {
            get
            {
                if (_rowHeightAccessor != null)
                {
                    object value = _rowHeightAccessor.GetValue(this);
                    if (value != null && value is int)
                    {
                        return (int)value;
                    }
                }
                return 22;
            }
        }

        #endregion hacked members

        #endregion props

        public void AddRow()
        {
            if (this.CurrencyManager == null || this.TableStyle == null) { return; }
            this.CurrencyManager.AddNew();
            this.Invalidate();
        }

        public void Edit()
        {
            EndEdit();//end any existing edits

            if (ReadOnly) { return; } //do nothing if dataGrid set to readOnly

            var col = CurrentCollumn as EditableColumnBase;

            if (col == null
                || !IsColumnDisplayable(col)) { return; }

            try
            {
                EditColumn = col;
                var curCell = CurrentCell;
                EditColumn.Edit(CurrencyManager
                    , curCell.RowNumber
                    , curCell.ColumnNumber);

                _isEditing = true;

                UpdateEditCell();
            }
            catch
            {
                //reset editing state, if all else fails
                EndEdit();
            }
        }

        private void CommitEdit()
        {
            if (!_isEditing || _inCommitEdit) { return; }

            var editCol = EditColumn;
            if (editCol != null)
            {
                _inCommitEdit = true;
                editCol.CommitEdit();
                _inCommitEdit = false;
            }
        }

        public void EndEdit()
        {
            if (!_isEditing) { return; }
            _isEditing = false;

            var editCol = EditColumn;
            if (editCol != null)
            {
                editCol.CommitEdit();
            }

            ////DataGrid recieves back focus only if it is Ending the edit
            ////that ways if the column loses focus to something other than the dataGrid,
            ////the dataGrid isn't fighting for focus
            //if (!_isScrolling)
            //{
            //    this.Focus();
            //}
        }

        public bool MoveFirstEmptyCell()
        {
            var rowIndex = this.CurrentRowIndex;
            if (rowIndex < 0) { return false; }
            if (this.TableStyle == null) { return false; }
            object rowData = this.CurrencyManager.List[rowIndex];
            for (int i = 0; i < this.TableStyle.GridColumnStyles.Count; i++)
            {
                var col = this.TableStyle.GridColumnStyles[i] as EditableColumnBase;
                if (col == null
                    || !this.IsColumnDisplayable(col)
                    || col.ReadOnly) { continue; }

                object cellValue = col.PropertyDescriptor.GetValue(rowData);
                if (cellValue == null)
                {
                    this.CurrentCell = new DataGridCell(rowIndex, i);
                    return true;
                }
                Type t = cellValue.GetType();
                if (cellValue is String && String.IsNullOrEmpty(cellValue as String))
                {
                    this.CurrentCell = new DataGridCell(rowIndex, i);
                    return true;
                }
                else if (t.IsValueType && object.Equals(cellValue, Activator.CreateInstance(t)))//if value is a value type(int, double, bool ....) compare cellValue to type's default vaule
                {
                    this.CurrentCell = new DataGridCell(rowIndex, i);
                    return true;
                }
            }
            return false;
        }

        public bool MoveSelectionUp()
        {
            var curRow = CurrentRowIndex;
            if (curRow > 0)
            {
                try
                {
                    this.CurrentRowIndex--;

                    EnsureCurrentCellFocused();
                }
                catch
                { return false; }

                return true;
            }
            return false;
        }

        public bool MoveSelectionDown()
        {
            return SelectNextRow();
        }

        public bool MoveSelectionLeft()
        {
            if (this.CurrentColumnIndex > 0)
            {
                this.CurrentColumnIndex--;

                if (!this.IsColumnDisplayable(this.CurrentCollumn))
                {
                    return MoveSelectionLeft();
                }
                else
                {
                    EnsureCurrentCellFocused();
                    return true;
                }
            }
            else
            { return false; }
        }

        public bool MoveSelectionRight()
        {
            if (CurrentColumnIndex < ColumnCount - 1)
            {
                this.CurrentColumnIndex++;

                if (!this.IsColumnDisplayable(this.CurrentCollumn))
                {
                    return MoveSelectionRight();
                }
                else
                {
                    EnsureCurrentCellFocused();
                    return true;
                }
            }
            else
            { return false; }
        }

        public bool SelectNextCell(bool forward)
        {
            //are we going forward from the last column
            if (forward && this.CurrentColumnIndex >= this.ColumnCount - 1)                         // are we going forward from the last column
            {
                return SelectNextRow();                                                             //go to next row
            }
            //are we going back from the first column
            else if (!forward && this.CurrentColumnIndex < 1 && this.CurrentRowIndex > 0)
            {
                //go to previous row
                this.CurrentCell = new DataGridCell(this.CurrentRowIndex - 1, this.ColumnCount - 1);
                this.EnsureCurrentCellFocused();
            }
            else
            {
                if (forward)
                {
                    MoveSelectionRight();
                }
                else
                {
                    MoveSelectionLeft();
                }
            }

            return true;
        }

        public bool SelectNextRow()
        {
            if ((this.CurrentRowIndex < this.RowCount - 1))                     //if we aren't at the end OR
            {
                this.CurrentCell = new DataGridCell(this.CurrentRowIndex + 1, this.CurrentColumnIndex);//select next cell in the next row
            }
            else if (this.CurrentRowIndex == this.RowCount - 1 && this.UserAddRow())//we are at the end and can make a new row
            {
                this.CurrentCell = new DataGridCell(this.CurrentRowIndex, 0);   //select first cell in the next row OR
            }
            else                                                                //we are at the end and can't make a new row
            {
                return false;
            }
            EnsureCurrentCellFocused();
            return true;
        }

        public bool UserAddRow()
        {
            if (AllowUserToAddRows)
            {
                this.AddRow();
                return true;
            }
            else
            {
                return false;
            }
        }

        

        protected void EnsureCurrentCellFocused()
        {
            EditableColumnBase col = this.TableStyle.GridColumnStyles[this.CurrentColumnIndex] as EditableColumnBase;
            if (col == null) //if current column doesn't have a control to have focus then, send focus to grid
            {
                this.Focus();
            }
            else
            {
                if (col.HostedControl.Focused == false)
                {
                    col.HostedControl.Focus();
                }
            }
        }

        public void ScrollToRow(int row)
        {
            if (row < 0 || row > RowCount) { return; }
            VertScrollBar.Value = row;
        }

        protected virtual void EnsureRowVisible(int row)
        {
            if (row < FirstVisibleRow || row > LastVisibleRow)
            {
                ScrollToRow(row);
            }
        }

        protected int GetDisplayableColumnCount()
        {
            int count = 0;
            if (this.TableStyle == null) { return 0; }
            for (int i = 0; i < this.TableStyle.GridColumnStyles.Count; i++)
            {
                if (this.IsColumnDisplayable(i))
                {
                    count++;
                }
            }
            return count;
        }

        protected bool IsColumnDisplayable(int colNum)
        {
            if (colNum < 0 || colNum > this.ColumnCount) { return false; }
            return this.IsColumnDisplayable(this.TableStyle.GridColumnStyles[colNum]);
        }

        protected bool IsColumnDisplayable(DataGridColumnStyle col)
        {
            if (col == null
                || col.Width <= 0
                || col.PropertyDescriptor == null)
            { return false; }
            else
            { return true; }
        }

        protected virtual void OnCellValidating(EditableDataGridCellValidatingEventArgs e)
        {
            if (CellValidating != null)
            {
                this.CellValidating(this, e);
            }
        }

        protected virtual void OnCellValueChanged(EditableDataGridCellEventArgs e)
        {
            if (this.CellValueChanged != null)
            {
                this.CellValueChanged(this, e);
            }
        }

        protected override void OnCurrentCellChanged(EventArgs e)
        {
            this.EndEdit();
            base.OnCurrentCellChanged(e);//causes bound dataSource to update current
            this.Edit();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            e.Handled = this.ProcessKeyPress(e.KeyData);
            if (e.Handled == true) { return; }
            base.OnKeyDown(e);
        }

        //listen to mouse clicks to allow IClickableDataGridColumns to handle MouseDown events
        protected override void OnMouseDown(MouseEventArgs mea)
        {
            base.OnMouseDown(mea);

            DataGrid.HitTestInfo hitTest;
            try
            {
                hitTest = this.HitTest(mea.X, mea.Y);
            }
            catch
            {
                return;
            }

            if (hitTest.Type == HitTestType.Cell
                && hitTest.Row > -1
                && hitTest.Column > -1)
            {
                DataGridTableStyle tableStyle = this.TableStyle;
                var column = tableStyle.GridColumnStyles[hitTest.Column];

                if (column != null && column is IClickableDataGridColumn)
                {
                    ((IClickableDataGridColumn)column).HandleMouseDown(hitTest.Row, mea);

                    var cellBounds = this.GetCellBounds(hitTest.Row, hitTest.Column);
                    this.Invalidate(cellBounds);
                    this.Update();
                }

                var curCell = CurrentCell;
                if (CurrentCell.ColumnNumber == hitTest.Column
                    && CurrentCell.RowNumber == hitTest.Row)
                {
                    Edit();
                }
            }
        }

        //listen to mouse clicks to allow IClickableDataGridColumns to handle Mouse click events
        protected override void OnMouseUp(MouseEventArgs mea)
        {
            base.OnMouseUp(mea);

            DataGrid.HitTestInfo hitTest;
            try
            {
                hitTest = this.HitTest(mea.X, mea.Y);
            }
            catch
            {
                return;
            }

            if (hitTest.Column > -1
                && hitTest.Row > -1
                && hitTest.Type == HitTestType.Cell)
            {
                DataGridTableStyle tableStyle = this.TableStyle;
                var column = tableStyle.GridColumnStyles[hitTest.Column] as IClickableDataGridColumn;

                if (column != null)                                                         //check that column is clickable, if so intercept click
                {                                                                           //dont call base.OnmouseUp if clickable because we dont want to Select the cell, just click it
                    column.HandleMouseClick(hitTest.Row);

                    var cellBounds = this.GetCellBounds(hitTest.Row, hitTest.Column);
                    this.Invalidate(cellBounds);
                    this.Update();
                }
            }
        }

        protected virtual void OnReadOnlyChanged()
        {
            if (this.ReadOnly)
            {
                EndEdit();
            }
            else
            {
                this.Edit();
            }
        }

        protected void UpdateEditCell()
        {
            if (EditColumn != null)
            {
                Rectangle bounds = Rectangle.Empty;
                bool isvisable = false;
                try
                {
                    bounds = this.GetCellBounds(this.EditColumn.EditRow, this.EditColumn.ColumnOrdinal);
                    isvisable = this.GridRec.IntersectsWith(bounds);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Write(e.ToString());
                }
                EditColumn.UpdateEditCell(bounds, isvisable);
            }
        }

        #region methods visable to the column
        //called by EditableColumnBase when edit control loses focus
        internal void OnEditControlLostFocus()
        {
            if (!this.Focused)
            {
                EndEdit();
            }
        }

        internal void OnCellValidatingInternal(EditableDataGridCellValidatingEventArgs e)
        {
            this.OnCellValidating(e);
        }

        internal void OnCellValueChangedInternal(EditableDataGridCellEventArgs e)
        {
            this.OnCellValueChanged(e);
        }
        #endregion

        private void DataSource_MetaDataChanged(object sender, EventArgs e)
        {
            try
            {
                this.EndEdit();
                this.CurrencyManager.EndCurrentEdit();
            }
            catch
            {
                /* do nothing */
            }
        }

        

        private void VertScrollBar_ValueChanged(Object sender, EventArgs e)
        {
            _isScrolling = true;
            try
            {
                this.UpdateEditCell();
            }
            finally
            {
                _isScrolling = false;
            }
        }

        private void WireDataSource()
        {
            CurrencyManager.MetaDataChanged += DataSource_MetaDataChanged;
        }

        private void UnwireDataSource()
        {
            this.CurrencyManager.MetaDataChanged -= DataSource_MetaDataChanged;
        }

        private void WireGridEvents()
        {
            VertScrollBar.ValueChanged += new EventHandler(this.VertScrollBar_ValueChanged);
            HorizScrollBar.ValueChanged += new EventHandler(this.VertScrollBar_ValueChanged);
        }

        private void UnwireGridEvents()
        {
            VertScrollBar.ValueChanged -= VertScrollBar_ValueChanged;
            HorizScrollBar.ValueChanged -= VertScrollBar_ValueChanged;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.UnwireGridEvents();
                
                ErrorBrush = null;
                AltTextBrush = null;
                ForePen = null;
            }
        }

        #region ISupportInitialize Members

        //HACK Code generator automaticly puts in calls to
        //theses methods and can't be fixed, so these methods
        //must be included but do nothing.
        public void BeginInit()
        { /*do nothing*/ }

        public void EndInit()
        { /*do nothing*/ }

        #endregion ISupportInitialize Members

        #region IKeyPressProcssor Members

        public bool ProcessBackTabKey()
        {
            return this.SelectNextCell(false);
        }

        public bool ProcessDialogKey(Keys keyVal)
        {
            switch (keyVal)
            {
                case (Keys.Tab):
                    {
                        return this.ProcessTabKey();
                    }
                case ((Keys.Left)):
                case ((Keys.Control | Keys.Left)):
                    {
                        return this.MoveSelectionLeft();
                    }
                case ((Keys.Down)):
                case ((Keys.Control | Keys.Down)):
                    {
                        return this.MoveSelectionDown();
                    }
                case ((Keys.Up)):
                case ((Keys.Control | Keys.Up)):
                    {
                        return this.MoveSelectionUp();
                    }
                case ((Keys.Right)):
                case ((Keys.Control | Keys.Right)):
                    {
                        return this.MoveSelectionRight();
                    }
            }

            //we dont know how to handle the key press
            //so, lets see if our parent want to
            IKeyPressProcessor p = this.Parent as IKeyPressProcessor;
            if (p == null) { return false; }
            return p.ProcessDialogKey(keyVal);
        }

        public bool ProcessKeyPress(Keys keyVal)
        {
            switch (keyVal)
            {
                case (Keys.Tab):
                case ((Keys.Left)):
                case ((Keys.Down)):
                case ((Keys.Up)):
                case ((Keys.Right)):
                case ((Keys.Control | Keys.Left)):
                case ((Keys.Control | Keys.Down)):
                case ((Keys.Control | Keys.Up)):
                case ((Keys.Control | Keys.Right)):
                    {
                        return this.ProcessDialogKey(keyVal);
                    }
            }
            //we dont know how to handle the key press
            //so, lets see if our parent want to
            IKeyPressProcessor p = this.Parent as IKeyPressProcessor;
            if (p == null) { return false; }
            return p.ProcessKeyPress(keyVal);
        }

        public bool ProcessReturnKey()
        {
            IClickableDataGridColumn col = this.CurrentCollumn as IClickableDataGridColumn;
            if (col != null)
            {
                col.HandleMouseClick(this.CurrentRowIndex);
                return true;
            }
            return false;
        }

        public bool ProcessTabKey()
        {
            return this.SelectNextCell(true);
        }

        #endregion IKeyPressProcssor Members
    }
}