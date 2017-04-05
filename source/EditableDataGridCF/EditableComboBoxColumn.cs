using System;
using System.Collections;
using System.Windows.Forms;

namespace EditableDataGridCF
{
    public class EditableComboBoxColumn : EditableColumnBase
    {
        #region Fields

        private string _displayMember;
        private string _valueMember;
        private object _dataSource;

        #endregion Fields

        #region Events

        public event EventHandler SelectedValueChanged;

        #endregion Events

        #region Properties

        public object DataSource
        {
            get
            {
                return _dataSource;
            }
            set
            {
                if (_dataSource == value) { return; }
                _dataSource = value;
                if (_hostedControl != null)
                {
                    try
                    {
                        if (value is IList)
                        {
                            EditComboBox.SetComboBoxItems((IList)value);
                        }
                        else
                        {
                            EditComboBox.SetComboBoxItems(new Object[] { value });
                        }
                    }
                    catch
                    {
                        _dataSource = null;
                    }
                }
            }
        }

        public string DisplayMember
        {
            get
            {
                return _displayMember;
            }
            set
            {
                if (_displayMember == value) { return; }
                _displayMember = value;
                if (base._hostedControl != null && EditComboBox != null)
                {
                    try
                    {
                        EditComboBox.DisplayMember = _displayMember;
                    }
                    catch
                    {
                        _displayMember = null;
                    }
                }
            }
        }

        public string ValueMember
        {
            get
            {
                return _valueMember;
            }
            set
            {
                if (_valueMember == value) { return; }
                _valueMember = value;
                if (base._hostedControl != null && this.EditComboBox != null)
                {
                    try
                    {
                        EditComboBox.ValueMember = _valueMember;
                    }
                    catch
                    {
                        _valueMember = null;
                    }
                }
            }
        }

        protected override bool CanShowHostedControlIfReadOnly
        {
            get { return false; }
        }

        internal EditableDataGridComboBox EditComboBox
        {
            get
            {
                return this.HostedControl as EditableDataGridComboBox;
            }
        }

        #endregion Properties

        internal override void CommitEdit()
        {
            this.EditComboBox.DroppedDown = false;
            base.CommitEdit();
        }

        protected override void UpdateHostedControl(object cellValue)
        {
            this.EditComboBox.Text = null;
            this.EditComboBox.Text = this.EditComboBox.GetItemText(cellValue);
        }

        protected override object GetHostControlValue()
        {
            string cellText = this.EditComboBox.Text;
            if (String.IsNullOrEmpty(cellText))
            {
                return null;
            }
            object value;
            if (this.EditComboBox.GetValueFromItemText(cellText, out value))//if the cell text matches a combobox item, get its value
            {
                return value;
            }
            else //otherwise try to convert the cell text
            {
                return base.ConvertValueFromText(cellText);
            }
        }

        internal void OnSelectedValueChanged(EventArgs e)
        {
            if (this.SelectedValueChanged != null)
            {
                this.SelectedValueChanged(this, e);
            }
        }

        protected override Control CreateHostedControl()
        {
            ComboBox box = new EditableDataGridComboBox();
            box.DataSource = _dataSource;
            box.DisplayMember = _displayMember;
            box.ValueMember = _valueMember;
            box.DropDownStyle = ComboBoxStyle.DropDown;
            return box;
        }
    }
}