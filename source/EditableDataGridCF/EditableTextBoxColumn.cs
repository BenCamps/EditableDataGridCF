using System;
using System.Drawing;
using System.Windows.Forms;

namespace EditableDataGridCF
{
    // This is our editable TextBox column.
    public class EditableTextBoxColumn : EditableColumnBase
    {
        private int _maxTextLength = -1;
        private int _textWidth;

        public EditableTextBoxColumn()
        { }

        /// <summary>
        /// If set to true, data grid will cycle to the next column when the length of text in the text box == MaxTextLength
        /// </summary>
        public bool GoToNextColumnWhenTextCompleate { get; set; }

        public int MaxTextLength
        {
            get { return this.TextBox.MaxLength; }
            set
            {
                if (base._hostedControl != null)
                {
                    this.TextBox.MaxLength = value;
                }
                this._maxTextLength = value;
            }
        }

        public bool MultiLine { get; set; }

        public virtual TextBox TextBox
        {
            get
            {
                return this.HostedControl as TextBox;
            }
        }

        protected void UnwireTextBoxEvents(TextBox textBox)
        {
            textBox.TextChanged -= TextBox_TextChanged;
        }

        protected void WireTextBoxEvents(TextBox textbox)
        {
            textbox.TextChanged += new EventHandler(TextBox_TextChanged);
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            EditableDataGrid dg = this.Owner as EditableDataGrid;
            if (dg != null)
            {
                _textWidth = dg.MeasureTextWidth(TextBox.Text);

                if (GoToNextColumnWhenTextCompleate &&
                  TextBox.TextLength == TextBox.MaxLength)
                {
                    dg.SelectNextCell(true);
                }
            }
            else
            {
                _textWidth = -1;
            }
        }

        #region override members

        protected override bool CanShowHostedControlIfReadOnly
        {
            get { return true; }
        }

        internal override void CommitEdit()
        {
            base.CommitEdit();
            //string text = this.TextBox.Text;
            //try
            //{
            //    object value = base.ConvertValueFromText(text);
            //    base.SetCellValue(base._row, value);
            //    base._preValue = value;
            //}
            //catch
            //{
            //    this.UpdateHostedControl(_preValue);
            //}
        }

        protected override Control CreateHostedControl()
        {
            TextBox box = new EditableDataGridTextBox();                                            // Our hosted control is a TextBox

            box.BorderStyle = BorderStyle.None;                                     // It has no border
            box.Multiline = this.MultiLine;
            box.ReadOnly = this.ReadOnly;

            box.AcceptsReturn = false;
            box.TextAlign = this.Alignment;                                         // Set up aligment.
            if (this._maxTextLength != -1) { box.MaxLength = _maxTextLength; }
            WireTextBoxEvents(box);

            return box;
        }

        protected override void DestroyHostedControl()
        {
            if (base._hostedControl != null)
            {
                UnwireTextBoxEvents(this.TextBox);
            }
            base.DestroyHostedControl();
        }

        internal override void Edit(CurrencyManager source, int row, int col)
        {
            base.Edit(source, row, col);
            this.TextBox.SelectAll();
        }

        protected override object GetHostControlValue()
        {
            try
            {
                return base.ConvertValueFromText(this.TextBox.Text);
            }
            catch
            {
                return string.Empty;
            }
        }

        protected override Rectangle GetPreferedBounds(Rectangle rc)
        {
            if (_textWidth > rc.Width)
            {
                if (this.MultiLine)
                {
                    rc.Height = rc.Height * ((_textWidth / rc.Width) + 1);
                }
                else
                {
                    rc.Width = _textWidth;
                }
            }
            return rc;
        }

        protected override void OnReadOnlyChanged()
        {
            base.OnReadOnlyChanged();
            if (TextBox != null)
            {
                this.TextBox.ReadOnly = base.ReadOnly;
            }
        }

        protected override void UpdateHostedControl(object cellValue)
        {
            this.TextBox.Text = base.FormatText(cellValue);
        }

        #endregion override members
    }
}