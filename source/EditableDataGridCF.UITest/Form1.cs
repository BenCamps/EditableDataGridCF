using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EditableDataGridCF.UITest
{
    public partial class Form1 : KeyPressDisatcherForm
    {
        EditableComboBoxColumn comboBoxCol;
        EditableTextBoxColumn textBoxCol;
        EditableDateTimePickerColumn dateTimeCol;
        EditableUpDownColumn upDwnCol;

        public Form1()
        {
            InitializeComponent();

            editableDataGrid1.CellValidating += new EditableDataGridCellValidatingEventHandler(editableDataGrid1_CellValidating);
            editableDataGrid1.CellValueChanged += new EditableDataGridCellValueChangedEventHandler(editableDataGrid1_CellValueChanged);

            //set up columns for our dataGrid
            DataGridTableStyle ts = new DataGridTableStyle() { MappingName = "something" };
            comboBoxCol = new EditableComboBoxColumn() { MappingName = "num1" };
            textBoxCol = new EditableTextBoxColumn() { MappingName = "word", MaxTextLength = 12, GoToNextColumnWhenTextCompleate = true };
            dateTimeCol = new EditableDateTimePickerColumn() { MappingName = "date" };
            upDwnCol = new EditableUpDownColumn() { MappingName = "upDwn" };
            ts.GridColumnStyles.Add(comboBoxCol);
            ts.GridColumnStyles.Add(textBoxCol);
            ts.GridColumnStyles.Add(dateTimeCol);
            ts.GridColumnStyles.Add(upDwnCol);

            //set up our button column and give it a event handler for button clicks
            //note the mapping name for the buttonColumn must be provided and a valid property to bind to
            //this is because the dataGrid only displays columns that have a valid MappingName
            //I've created a work around that should work for Full Framework, and will probably be able to make a workaround for Mobile
            //, but until then provide a mapping name.
            //For DAL DataObjects you could use the Tag property as a mapping name, since it isn't reserved for any propuse, and is intended for special cases like this.
            DataGridButtonColumn buttonCol = new DataGridButtonColumn() { MappingName = "button", UseCellColumnTextForCellValue = true, Text = "click me" };
            buttonCol.Click += new ButtonCellClickEventHandler(buttonCol_Click);
            ts.GridColumnStyles.Add(buttonCol);

            editableDataGrid1.TableStyles.Add(ts);

            //create some dummy data to test the grid
            List<something> things = new List<something>();
            for (int i = 0; i < 100; i++)
            {
                things.Add(new something() { num1 = i, word = "word", button = "click me" });
            }
            bindingSource1.DataSource = things;
            editableDataGrid1.DataSource = bindingSource1;
        }

        void editableDataGrid1_CellValueChanged(object sender, EditableDataGridCellEventArgs e)
        {
            label1.Text = e.RowIndex.ToString();
        }

        void editableDataGrid1_CellValidating(object sender, EditableDataGridCellValidatingEventArgs e)
        {
            label1.Text = e.Value.ToString();
        }

        void buttonCol_Click(ButtonCellClickEventArgs e)
        {
            label1.Text = e.RowNumber.ToString();
        }

        public class something : IDataErrorInfo
        {
            public int num1 { get; set; }

            public string word { get; set; }

            public string button { get; set; }

            public DateTime date { get; set; }

            public int upDwn { get; set; }

            #region IDataErrorInfo Members

            public string Error
            {
                get { return "error"; }
            }

            public string this[string columnName]
            {
                get { return columnName + " error"; }
            }

            #endregion IDataErrorInfo Members
        }
    }
}