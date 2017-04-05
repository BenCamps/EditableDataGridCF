using System;
using System.Windows.Forms;

namespace EditableDataGridCF
{
    

    public class EditableUpDownColumn : EditableColumnBase
    {


        // Let's add this so user can access control
        public virtual NumericUpDown NumericUpDown
        {
            get { return this.HostedControl as NumericUpDown; }
        }

        protected override bool CanShowHostedControlIfReadOnly
        {
            get { return false; }
        }

        #region overriden methods
        protected override Control CreateHostedControl()
        {
            NumericUpDown nud = new EditableDataGridNumericUpDown();
            nud.Minimum = -1;

            return nud;
        }

        internal override void CommitEdit()
        {
            base.CommitEdit();
            //object value = this.NumericUpDown.Value;
            //try
            //{
            //    this.SetCellValue(_row, value);
            //}
            //catch
            //{
            //    this.UpdateHostedControl(_preValue);
            //}
        }

        protected override object GetHostControlValue()
        {
            return this.NumericUpDown.Value;
        }

        protected override void UpdateHostedControl(object cellValue)
        {
            try
            {
                this.NumericUpDown.Value = Convert.ToDecimal(cellValue);
            }
            catch
            {
                this.NumericUpDown.Value = default(decimal);
            }
        }

        #endregion
    }


    internal class EditableDataGridNumericUpDown : NumericUpDown, IKeyPressProcessor
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

                case (Keys.Up):
                case (Keys.Down):
                    {
                        return false;   //let the up down control handle the behavior
                    }
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
                case ((Keys.Control | Keys.Left)):
                case ((Keys.Control | Keys.Down)):
                case ((Keys.Control | Keys.Up)):
                case ((Keys.Control | Keys.Right)):
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
