using System.Windows.Forms;

namespace EditableDataGridCF
{
    public class KeyPressDisatcherForm : Form
    {
        public KeyPressDisatcherForm()
            : base()
        {
            this.KeyPreview = true;
        }

        private KeyPressDispatcher _keyPressDispatcher;

        protected KeyPressDispatcher KeyDispatcher
        {
            get
            {
                if (_keyPressDispatcher == null)
                {
                    _keyPressDispatcher = new KeyPressDispatcher(this);
                }
                return _keyPressDispatcher;
            }
        }

        //by setting handled to true prevents the from from handeling the tab key or any other control from recieving OnKeyDown
        //, but it will still fire OnKeyPress
        protected override void OnKeyDown(KeyEventArgs e)
        {
            //provide a case for each key press we want to handle
            switch (e.KeyData)
            {
                case (Keys.Tab):
                    {
                        if (e.Modifiers == Keys.Shift)
                        {
                            e.Handled = this.KeyDispatcher.ProcessTabKey();
                        }
                        else
                        {
                            e.Handled = this.KeyDispatcher.ProcessTabKey();
                        }
                        break;
                    }
                case (Keys.Return):
                    {
                        e.Handled = this.KeyDispatcher.ProcessReturnKey();
                        break;
                    }
                case (Keys.Shift | Keys.Tab):
                    {
                        e.Handled = this.KeyDispatcher.ProcessBackTabKey();
                        break;
                    }
                default:
                    {
                        //optional general case for any key press
                        base.OnKeyDown(e);
                        break;
                    }
            }
        }
    }
}
