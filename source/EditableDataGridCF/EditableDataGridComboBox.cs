using System;
using System.Collections;
using System.Windows.Forms;

namespace EditableDataGridCF
{
    internal class EditableDataGridComboBox : ComboBox
    {
        public bool DroppedDown
        {
            get
            {
                IntPtr hndl = this.Handle;
                if (hndl == IntPtr.Zero) { return false; } //handle has not been created
                return Win32.SendMessage(hndl, Win32.CB_GETDROPPEDSTATE, 1, 0) != 0;
            }
            set
            {
                Win32.SendMessage(this.Handle, Win32.CB_SHOWDROPDOWN, value ? 1 : 0, 0);
            }
        }

        //public new object SelectedItem
        //{
        //    get
        //    {
        //        string itemText = this.Text;
        //        int i = this.FindItem(itemText, true);
        //        if (i < 0)
        //        {
        //            return null;
        //        }
        //        else
        //        {
        //            return this.Items[i];
        //        }
        //    }
        //    set
        //    {
        //        base.SelectedItem = value;
        //    }

        //}

        public new object SelectedItem
        {
            get
            {
                string itemText = this.Text;
                if (String.IsNullOrEmpty(itemText))
                {
                    return null;
                }
                else
                {
                    object value;
                    if (this.GetValueFromItemText(itemText, out value))
                    {
                        return value;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            set
            {
                base.SelectedItem = value;
            }
        }

#if NetCF_20

        /// <summary>
        /// Gets and Sets the location of the cursor in the textbox portion of the combobox
        /// </summary>
        public int SelectionStart
        {
            get
            {
                IntPtr hndl = this.Handle;
                int wpram = 0;
                int returnval;
                if (hndl == IntPtr.Zero) { return 0; }
                {
                    returnval = Win32.SendMessage(hndl, 320, wpram, 0);// close, first few bits of return is the actual index perhaps it has something to do with textbox max size of ?? 32766 ( look it up!)
                    returnval = returnval & 0xFFFF;         //use 16 bit mask to get actual value
                }
                return returnval;
            }

            set
            {
                this.Select(value, 0);
            }
        }

#endif

        

        public object GetValueFromItemText(String displayValue)
        {
            int index = this.FindItem(displayValue, true);
            if (index == -1) { return null; }
            object item = base.Items[index];
            object itemValue = base.FilterItemOnProperty(item, this.ValueMember);
            if (itemValue != null)
            {
                item = itemValue;
            }
            return item;
        }

        public int FindItem(string s, bool ignoreCase)
        {
            IList items = (IList)this.Items;
            for (int i = 0; i < items.Count; i++)
            {
                if (String.Compare(this.GetItemText(items[i]), s, ignoreCase) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool GetValueFromItemText(String displayValue, out object itemValue)
        {
            itemValue = null;
            int index = this.FindItem(displayValue, true);
            if (index == -1) { return false; }
            object item = base.Items[index];
            if (!String.IsNullOrEmpty(this.ValueMember))
            {
                itemValue = base.FilterItemOnProperty(item, this.ValueMember);
            }
            else
            {
                itemValue = item;
            }
            return true;
        }

#if NetCF_20

        /// <summary>
        /// Programaticly select text in the textbox portion of the combobox
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length"></param>
        public void Select(int start, int length)
        {
            if (start < 0)
            {
                throw new IndexOutOfRangeException("start");
            }
            else if (length < 0)
            {
                throw new ArgumentException("InvalidArgument", "length");
            }
            else
            {
                IntPtr hndl = this.Handle;
                if (hndl != IntPtr.Zero)
                {
                    int high = start + length;
                    Win32.SendMessage(hndl, Win32.CB_SETEDITSEL, 0, Win32.MAKELPARAM(start, high).ToInt32());
                }
            }
        }

#endif

        public void SetComboBoxItems(System.Collections.IList list)
        {
            this.SetItemsCore(list);
        }







        protected override void OnGotFocus(EventArgs e)
        {
            _keyCounter = 0;
            base.OnGotFocus(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            e.Handled = this.ProcessKeyPress(e.KeyData);
            if (e.Handled == false)
            {
                base.OnKeyDown(e);
            }

        }

        #region IkeyPressProcessor members

        private int _keyCounter = 0;//keeps track of the number of times the user hits enter on the control after it gains focus. 
        public virtual bool ProcessReturnKey()
        {

            if (this.DroppedDown == true)
            {
                _keyCounter++;
                EditableDataGrid dg = this.Parent as EditableDataGrid;

                if(true // (DeviceInfo.DevicePlatform == PlatformType.WinCE //TODO
                    && (dg != null && _keyCounter == 3))
                {
                    dg.MoveSelectionRight();
                    return true;
                }

                else if (dg != null && _keyCounter == 2)
                {
                    dg.MoveSelectionRight();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                _keyCounter = 0;

                IKeyPressProcessor p = this.Parent as IKeyPressProcessor;
                if (p == null) { return false; }
                return p.ProcessReturnKey();
            }
        }

        public virtual bool ProcessTabKey()
        {
            IKeyPressProcessor p = this.Parent as IKeyPressProcessor;
            if (p == null) { return false; }
            return p.ProcessTabKey();
        }

        public virtual bool ProcessBackTabKey()
        {
            IKeyPressProcessor p = this.Parent as IKeyPressProcessor;
            if (p == null) { return false; }
            return p.ProcessBackTabKey();
        }

        public virtual bool ProcessDialogKey(Keys keyVal)
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
                        if (this.DroppedDown == false)
                        {
                            EditableDataGrid dg = this.Parent as EditableDataGrid;
                            if (dg != null)
                            {
                                return dg.MoveSelectionUp();
                            }
                        }
                        break;
                    }
                case (Keys.Down):
                    {
                        if (this.DroppedDown == false)
                        {
                            EditableDataGrid dg = this.Parent as EditableDataGrid;
                            if (dg != null)
                            {
                                return dg.MoveSelectionDown();
                            }
                        }
                        break;
                    }

                default:
                    {
                        IKeyPressProcessor p = this.Parent as IKeyPressProcessor;
                        if (p == null) { return false; }
                        return p.ProcessDialogKey(keyVal);
                    }
            }
            return false;
        }

        public virtual bool ProcessKeyPress(Keys keyVal)
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
        #endregion
    }
}