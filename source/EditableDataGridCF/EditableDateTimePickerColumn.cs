using System.Windows.Forms;
using System;

namespace EditableDataGridCF
{
    // This is our editable TextBox column.
    public class EditableDateTimePickerColumn : EditableColumnBase
    {

        protected override bool CanShowHostedControlIfReadOnly
        {
            get { return false; }
        }
        // Let's add this so user can access 
        public virtual DateTimePicker DateTimePicker
        {
            get { return this.HostedControl as DateTimePicker; }
        }

        #region overriden methods
        protected override Control CreateHostedControl()                            
        {
            DateTimePicker dtp = new EditableDataGridDateTimePicker();                                  // Our hosted control is a DTP
            
            dtp.Format = DateTimePickerFormat.Short;
            
            return dtp;
        }

        internal override void CommitEdit()
        {
            base.CommitEdit();
            //object value = this.DateTimePicker.Value;
            //try
            //{
            //    base.SetCellValue(_row, value);
            //}
            //catch
            //{
            //    this.UpdateHostedControl(_preValue);
            //}
        }

        protected override object GetHostControlValue()
        {
            return this.DateTimePicker.Value;
        }

        protected override void UpdateHostedControl(object cellValue)
        {
            try
            {
                this.DateTimePicker.Value = Convert.ToDateTime(cellValue);
            }
            catch
            {
                this.DateTimePicker.Value = DateTimePicker.MinDate;
            }
        }

        #endregion
    }

    internal class EditableDataGridDateTimePicker : DateTimePicker, IKeyPressProcessor
    {

        protected override void OnKeyDown(KeyEventArgs e)
        {
            e.Handled = this.ProcessKeyPress(e.KeyData);
            if (e.Handled == false)
            {
                base.OnKeyDown(e);
            }
        }

        #region IkeyPressProcessor members

        public bool ProcessDialogKey(Keys keyVal)
        {
            switch (keyVal)
            {
                case (Keys.Left):
                case (Keys.Right):
                case (Keys.Up):
                case (Keys.Down):
                default:
                    {
                        IKeyPressProcessor p = this.Parent as IKeyPressProcessor;
                        if (p == null) { return false; }
                        return p.ProcessDialogKey(keyVal);
                    }
            }

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
            switch (keyVal)
            {
                case (Keys.Up):
                case (Keys.Down):
                case (Keys.Left):
                case (Keys.Right):
                    {
                        return this.ProcessDialogKey(keyVal);
                    }
                default:
                    {
                        IKeyPressProcessor p = this.Parent as IKeyPressProcessor;
                        if (p == null) { return false; }
                        return p.ProcessKeyPress(keyVal);
                    }
            }
        }
        #endregion
    }
}


