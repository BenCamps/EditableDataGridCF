using System.Windows.Forms;

namespace EditableDataGridCF
{
    internal class EditableDataGridTextBox : TextBox, IKeyPressProcessor
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            e.Handled = this.ProcessKeyPress(e.KeyData);
            if (e.Handled == false)
            {
                base.OnKeyDown(e);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            //prevents textbox from signaling key press as invalid action
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                return;
            }

            base.OnKeyPress(e);
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
                        return ProcessDialogKey(keyVal);
                    }
                case (Keys.Enter):
                    {
                        return ProcessReturnKey();
                    }
                default:
                    {
                        IKeyPressProcessor p = this.Parent as IKeyPressProcessor;
                        if (p == null) { return false; }
                        return p.ProcessKeyPress(keyVal);
                    }
            }
        }

        public bool ProcessDialogKey(Keys keyVal)
        {
            switch (keyVal)
            {
                case (Keys.Left):
                    {
                        if (this.SelectionStart == 0)
                        {
                            EditableDataGrid dg = this.Parent as EditableDataGrid;
                            if (dg != null)
                            {
                                return dg.MoveSelectionLeft();
                            }
                        }
                        break;
                    }
                case (Keys.Right):
                    {
                        if (this.SelectionStart == this.Text.Length)
                        {
                            EditableDataGrid dg = this.Parent as EditableDataGrid;
                            if (dg != null)
                            {
                                return dg.MoveSelectionRight();
                            }
                        }
                        break;
                    }
                case (Keys.Up):
                    {
                        EditableDataGrid dg = this.Parent as EditableDataGrid;
                        if (dg != null)
                        {
                            return dg.MoveSelectionUp();
                        }
                        break;
                    }
                case (Keys.Down):
                    {
                        EditableDataGrid dg = this.Parent as EditableDataGrid;
                        if (dg != null)
                        {
                            return dg.MoveSelectionDown();
                        }
                        break;
                    }
                default:
                    {
                        return false;
                    }
            }
            return false;
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
            return this.ProcessTabKey();
        }

    }
}