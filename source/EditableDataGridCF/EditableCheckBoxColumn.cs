using System;
using System.Drawing;
using System.Windows.Forms;


namespace EditableDataGridCF
{
    public class EditableCheckBoxColumn : EditableColumnBase
    {
        private object _nullValue = DBNull.Value;
        //private bool _threeState;

        //public event EventHandler CheckStateChanged;

        #region Properties 
        //public bool ThreeState
        //{
        //    get
        //    {
        //        return _threeState;
        //    }
        //    set
        //    {
        //        _threeState = value;
        //        if (base._hostedControl != null)
        //        {
        //            this.CheckBox.ThreeState = value;
        //        }
        //    }
        //}

        protected override bool CanShowHostedControlIfReadOnly
        {
            get { return false; }
        }

        public virtual CheckBox CheckBox
        {
            get { return this.HostedControl as CheckBox; }
        }

        #endregion

        #region overriden methods
        protected override Control CreateHostedControl()
        {
            CheckBox box = new EditableDataGridCheckBox();                             
            //box.ThreeState = _threeState;       
            return box;
        }

        internal override void CommitEdit()
        {
            base.CommitEdit();
            //object isChecked = this.CheckBox.Checked;
            //try
            //{
            //    base.SetCellValue(_row, isChecked);
            //    if (isChecked == null && _preValue != null || isChecked != null && _preValue == null || isChecked != null && !_preValue.Equals(isChecked))
            //    {
            //        this.OnCheckStateChanged(new EventArgs());
            //    }
            //}
            //catch
            //{
            //    this.UpdateHostedControl(_preValue);
            //}
        }

        protected override object GetHostControlValue()
        {
            return this.CheckBox.Checked;
        }

        protected override void UpdateHostedControl(object cellValue)
        {
            bool value;
            try
            {
                value = (bool)cellValue;
            }
            catch
            {
                value = false;
            }
            this.CheckBox.Checked = value;
        }

        #endregion

        //protected void OnCheckStateChanged( EventArgs e)
        //{
        //    if (this.CheckStateChanged != null)
        //    {
        //        this.CheckStateChanged(this, e);
        //    }
        //}

        protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
        {
            Object cellData;                                                    // Object to show in the cell 

            DrawBackground(g, bounds, rowNum, backBrush);                       // Draw cell background

            cellData = this.PropertyDescriptor.GetValue(source.List[rowNum]);   // Get data for this cell from data source.
            //String cellText = FormatText(cellData);
            if((cellData != null) && (cellData != this.NullValue) && (cellData is IConvertible))
            {                                                                   // Data is IConvertale and not NULL?
                cellData = ((IConvertible)cellData).ToBoolean(this.FormatInfo); // Go ahead and convert it to boolean
            }
            
            DrawCheckBox(g, bounds, (cellData is Boolean )? ((bool)cellData ? CheckState.Checked : CheckState.Unchecked) : CheckState.Indeterminate);
                                                                                // Draw the checkbox according to the data in data source.

            //this.UpdateHostedControl(null);                                         // Have to do that.
        }
 

        private void DrawCheckBox(Graphics g, Rectangle bounds, CheckState state)
        {
            // If I Were A Painter... That would look way better. Sorry.

            int size;
            int boxTop;
                
            size = bounds.Size.Height < bounds.Size.Width ? bounds.Size.Height : bounds.Size.Width;
            size = size > ((int)g.DpiX / 7) ? ((int)g.DpiX / 7) : size;

            boxTop = bounds.Y + (bounds.Height - size) / 2;
            
            using (Pen p = new Pen(this.Owner.ForeColor)) 
            {
                g.DrawRectangle(p, bounds.X, boxTop, size, size);
            }

            if (state != CheckState.Unchecked) 
            {
                using (Pen p = new Pen(state == CheckState.Indeterminate ? SystemColors.GrayText : SystemColors.ControlText)) 
                {
                    g.DrawLine(p, bounds.X, boxTop, bounds.X + size, boxTop + size);
                    g.DrawLine(p, bounds.X, boxTop + size, bounds.X + size, boxTop);
                }
            }
        }
    }

    internal class EditableDataGridCheckBox : CheckBox, IKeyPressProcessor
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            this.ProcessKeyPress(e.KeyData);
            if (e.Handled == false)
            {
                base.OnKeyDown(e);
            }
        }

    #region IkeyPressProcessor members 

        public bool ProcessDialogKey(Keys keyVal)
        {
            IKeyPressProcessor p = this.Parent as IKeyPressProcessor;
            if (p == null) { return false; }
            return p.ProcessDialogKey(keyVal);
        }

        public bool ProcessTabKey()
        {
            IKeyPressProcessor p = this.Parent as IKeyPressProcessor;
            if (p == null) { return false; }
            return p.ProcessTabKey();
        }

        public bool ProcessBackTabKey()
        {
            IKeyPressProcessor p = this.Parent as IKeyPressProcessor;
            if (p == null) { return false; }
            return p.ProcessBackTabKey();
        }

        public bool ProcessReturnKey()
        {
            IKeyPressProcessor p = this.Parent as IKeyPressProcessor;
            if (p == null) { return false; }
            return p.ProcessReturnKey();
        }

        public bool ProcessKeyPress(Keys keyVal)
        {
            IKeyPressProcessor p = this.Parent as IKeyPressProcessor;
            if (p == null) { return false; }
            return p.ProcessKeyPress(keyVal);
        }
    #endregion 
    }
}
