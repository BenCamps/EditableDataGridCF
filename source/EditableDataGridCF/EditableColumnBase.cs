using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace EditableDataGridCF
{
    // We'll inherit from DataGridTextBoxColumn, not from DataGridColumnStyle to ensure desktop compatibility.
    // Since some abstract methods are not availible on NETCF's DataGridColumnStyle, it's not possible to override them.
    // Thus attempt to run this code on desktop would fail as these abstract methods won't have implementation at runtime.
    public abstract partial class EditableColumnBase : CustomColumnBase
    {
        private readonly static int ERROR_ICON_WIDTH = 10;
        protected Rectangle _bounds = Rectangle.Empty;
        protected Control _hostedControl = null;
        protected bool _isEditing;
        protected object _orgValue;

        //protected int _row;
        protected System.Reflection.MethodInfo _parseMethod;

        // Column's hosted control (e.g. TextBox).
        // Last known bounds of hosted control.
        private bool _readOnly = false;

        public int EditRow { get; private set; }

        // ReadOnly state
        public virtual Control HostedControl
        {
            get
            {
                if ((null == this._hostedControl) && (this.Owner != null))
                {
                    this._hostedControl = CreateHostedControl();
                    InitializeHostControl();
                }

                return _hostedControl;
            }
        }

        public new EditableDataGrid Owner
        {
            get
            {
                return base.Owner as EditableDataGrid;
            }
        }

        #region abstract members

        // Sub-Class can decide if host control can be show in read only mode.
        // Use this if host control has its own read only property
        protected abstract bool CanShowHostedControlIfReadOnly { get; }

        // Creates hosted control and sets it's properties as needed.
        protected abstract Control CreateHostedControl();

        //gets the hosted controls current value
        protected abstract object GetHostControlValue();

        //updates hosted control
        protected abstract void UpdateHostedControl(object cellValue);

        #endregion abstract members

        #region override members

        public override bool ReadOnly
        {
            get
            {
                return this._readOnly;
            }
            set
            {
                if (_readOnly == value) { return; }
                this._readOnly = value;
                OnReadOnlyChanged();
                this.Invalidate();
            }
        }

        public override PropertyDescriptor PropertyDescriptor
        {
            get
            {
                return base.PropertyDescriptor;
            }
            set
            {
                base.PropertyDescriptor = value;
                if (this.PropertyDescriptor != null && this.PropertyDescriptor.PropertyType != typeof(object))
                {
                    this._parseMethod = this.PropertyDescriptor.PropertyType.GetMethod("Parse", new Type[] { typeof(string), typeof(IFormatProvider) });
                }
                else
                {
                    this._parseMethod = null;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._hostedControl != null)
                {
                    this._hostedControl.Dispose();
                    this._hostedControl = null;
                }
            }

            base.Dispose(disposing);
        }

        protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
        {
            RectangleF textBounds;                                              // Bounds of text
            Object cellData;                                                    // Object to show in the cell
            Object rowData = source.List[rowNum];

            cellData = this.PropertyDescriptor.GetValue(source.List[rowNum]);   // Get data for this cell from data source.
            String cellText = FormatText(cellData);

            DrawBackground(g, bounds, rowNum, backBrush);
            IDataErrorInfo errorInfo = rowData as IDataErrorInfo;
            if (Owner is EditableDataGrid &&                                    //Draw ErrorInfo if Data Grid is compatable
                ((EditableDataGrid)Owner).ErrorColor != null &&
                errorInfo != null &&
                string.IsNullOrEmpty(errorInfo[this.MappingName]) == false)
            {
                Brush errorBrush = (Owner as EditableDataGrid).ErrorBrush;
                DrawCellError(g, bounds, errorBrush, foreBrush, errorInfo[MappingName]);
            }

            bounds.Inflate(-2, -2);                                             // Shrink cell by couple pixels for text.

            textBounds = new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            // Set text bounds.

            g.DrawString(cellText, this.Owner.Font, foreBrush, textBounds, this.StringFormat);
            // Render contents
        }

        #endregion override members

        internal virtual void AbortEdit()
        {
            this.RevertEdit();
            this.EndEdit();
            this.ConceadeFocus();
        }

        internal virtual void CommitEdit()
        {
            if (this.ReadOnly) { return; }

            try
            {
                System.Diagnostics.Debug.Assert(EditRow != -1);

                object value = GetHostControlValue();
                bool hasChanges = !CompareValues(_orgValue, value);
                if (hasChanges)
                {
                    bool cancel = NotifyCellValidating(value);
                    if (cancel == true) { return; }

                    base.SetCellValue(this.EditRow, value);
                    _orgValue = value;
                    this.NotifyCellValueChanged();
                }
            }
            catch
            {
                RevertEdit();
            }
        }

        internal virtual void HideEditControl()
        {
            HostedControl.Bounds = Rectangle.Empty;

            bool isFocused = HostedControl.Focused;
            if (isFocused && Owner != null)
            {
                Owner.Focus();
            }
        }

        internal virtual bool ConceadeFocus()
        {
            bool isFocused = HostedControl.Focused;
            if (isFocused && Owner != null)
            {
                Owner.Focus();
                return true;
            }
            return false;
        }

        internal virtual object ConvertValueFromText(String text)
        {
            object value;
            Type propType = this.PropertyDescriptor.PropertyType;

            try
            {
                if (!String.IsNullOrEmpty(base.Format) && this._parseMethod != null && this.FormatInfo != null)
                {
                    value = this._parseMethod.Invoke(null, new object[] { text, this.FormatInfo });
                }
                else
                {
                    if (string.IsNullOrEmpty(text))
                    {
                        if (propType.IsValueType)
                        {
                            value = Activator.CreateInstance(propType);
                        }
                        else
                        {
                            value = null;
                        }
                        return value;
                    }
                    value = Convert.ChangeType(text, propType, this.FormatInfo);
                }
            }
            catch
            {
                value = null;
            }
            return value;
        }

        internal virtual void Edit(CurrencyManager source, int row, int column)
        {
            if (!_isEditing) //if cell is just entering edit
            {
                object cellValue = base.GetCellValue(source, row);
                _orgValue = cellValue;
                UpdateHostedControl(cellValue);
                HostedControl.Focus();
            }
            _isEditing = true;

            this.EditRow = row;
        }

        internal virtual void EndEdit()
        {
            _isEditing = false;
            EditRow = -1;

            HideEditControl();
        }

        void InitializeHostControl()
        {
            //this._hostedControl.Validating += new CancelEventHandler(this.OnValidating);
            //this._hostedControl.TextChanged += new EventHandler(this.OnTextChanged);

            this._hostedControl.Visible = false;                                // Hide it.
            this._hostedControl.Name = this.HeaderText;
            this._hostedControl.Font = this.Owner.Font;                         // Set up control's font to match grid's font.
            this._hostedControl.BackColor = this.Owner.SelectionBackColor;      //make sure our control has the same back color as the data grid
            this._hostedControl.ForeColor = this.Owner.SelectionForeColor;

            this._hostedControl.LostFocus += new EventHandler(_hostedControl_LostFocus);
            this._hostedControl.Parent = this.Owner;

            //_hostControlBinding = new Binding(this.GetBoundPropertyName(), Owner.DataSource, this.MappingName, true, DataSourceUpdateMode.OnValidation, this.NullValue, this.Format, this.FormatInfo);
            //_hostControlBinding.Parse += new ConvertEventHandler(binding_Parse);
            //this._hostedControl.DataBindings.Add(_hostControlBinding);
            switch (Type.GetTypeCode(this.PropertyDescriptor.PropertyType))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    {
                        Microsoft.WindowsCE.Forms.InputModeEditor.SetInputMode(_hostedControl, Microsoft.WindowsCE.Forms.InputMode.Numeric);
                        break;
                    }
            }
        }

        internal virtual void RevertEdit()
        {
            this.UpdateHostedControl(_orgValue);
        }

        internal virtual void UpdateEditCell(Rectangle bounds, bool isVisable)
        {
            isVisable = isVisable && (!this.ReadOnly || this.CanShowHostedControlIfReadOnly);
            if (isVisable)
            {
                _bounds = GetPreferedBounds(bounds);
                HostedControl.Bounds = _bounds;
                HostedControl.Visible = true;
                //HostedControl.Focus();//for combobox calling focus causes controll to lose focus if already focused
            }
            else
            {
                HostedControl.Bounds = Rectangle.Empty;//hides control without affecting focus control
            }
        }

        protected static bool CompareValues(object obj1, object obj2)
        {
            bool objEquals = !(obj2 == null && obj1 != null
                || obj2 != null && obj1 == null
                || obj2 != null && !obj1.Equals(obj2)
                || obj1 != null && !obj2.Equals(obj1));

            //bool compEquals = false;
            //if (obj1 is IComparable)
            //{
            //    compEquals = ((IComparable)obj1).CompareTo(obj2) == 0;
            //}
            //if (!compEquals && obj2 is IComparable)
            //{
            //    compEquals = ((IComparable)obj2).CompareTo(obj1) == 0;
            //}
            return objEquals;
        }

        protected virtual void DestroyHostedControl()
        {
            if (_hostedControl != null)
            {
                Owner.Controls.Remove(_hostedControl);
                _hostedControl.Dispose();
                _hostedControl = null;
            }
        }

        protected virtual void DrawBackground(Graphics g, Rectangle bounds, int rowNum, Brush backBrush)
        {
            Brush background = backBrush;                                       // Use default brush by... hmm... default.

            if ((Owner is EditableDataGrid) &&                                   //owner is a editable datagrid
                (((EditableDataGrid)Owner).AlternatingBackBrush != null) &&         //has a alternating brush
                ((rowNum & 1) != 0) &&                                          //row number is odd
                !Owner.IsSelected(rowNum))                                      //and row is not selected
            {
                background = (Owner as EditableDataGrid).AlternatingBackBrush;
            }

            g.FillRectangle(background, bounds);                                // Draw cell background
        }

        protected virtual void DrawCellError(Graphics g, Rectangle cellBounds, Brush errorBrush, Brush textBrush, String errorMessage)
        {
            Rectangle errorIconBounds = new Rectangle(cellBounds.Right - ERROR_ICON_WIDTH, cellBounds.Top, ERROR_ICON_WIDTH, cellBounds.Height);
            g.FillRectangle(errorBrush, errorIconBounds);
            g.DrawString(" !", Owner.Font, textBrush, errorIconBounds);
        }

        protected virtual Rectangle GetPreferedBounds(Rectangle rc)
        {
            return rc;
        }

        protected virtual bool NotifyCellValidating(object value)
        {
            EditableDataGridCellValidatingEventArgs e = new EditableDataGridCellValidatingEventArgs()
            {
                Column = this,
                RowIndex = this.EditRow,
                Value = value,
            };

            Owner.OnCellValidatingInternal(e);
            return e.Cancel;
        }

        protected virtual void NotifyCellValueChanged()
        {
            EditableDataGridCellEventArgs e = new EditableDataGridCellEventArgs()
            {
                Column = this,
                RowIndex = this.EditRow
            };
            Owner.OnCellValueChangedInternal(e);
        }

        protected virtual void OnReadOnlyChanged()
        {
        }

        void _hostedControl_LostFocus(object sender, EventArgs e)
        {
            Owner.OnEditControlLostFocus();
        }

        //void binding_Parse(object sender, ConvertEventArgs e)
        //{
        //    if (e.Value is string && string.IsNullOrEmpty((string)e.Value))
        //    {
        //        if (e.DesiredType.IsValueType)
        //        {
        //            object defaultValue = Activator.CreateInstance(e.DesiredType);
        //            e.Value = defaultValue;
        //        }
        //    }
        //}
    }
}