using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace EditableDataGridCF
{
    public class DataGridButtonColumn : CustomColumnBase, IClickableDataGridColumn
    {
        private bool _mouseDown = false;
        private int _mouseRowNum = -1; 

        private PropertyDescriptor _defaultPropertyDescriptor;
        protected PropertyDescriptor DefaultPropertyDescriptor
        {
            get
            {
                if (_defaultPropertyDescriptor == null)
                {
                    _defaultPropertyDescriptor = CreateDefaultPropertyDescriptor();
                }
                return _defaultPropertyDescriptor;
            }
        }

        private Pen _boarderPen; 
        protected Pen BoarderPen
        {
            get
            {
                if (_boarderPen == null && this.Owner != null)
                {
                    this._boarderPen = new Pen(this.Owner.ForeColor);
                }
                return _boarderPen; 
            }

        }   

        public event ButtonCellClickEventHandler Click ;

        public String Text { get; set; }

        public override PropertyDescriptor PropertyDescriptor
        {
            get
            {
                if (UseCellColumnTextForCellValue || string.IsNullOrEmpty(this.MappingName))
                {
                    return null;
                }
                else
                {
                    return base.PropertyDescriptor;
                }
            }
            set
            {
                base.PropertyDescriptor = value;
            }
        }

        public bool UseCellColumnTextForCellValue { get; set; }        

        public DataGridButtonColumn()
        {
          
        }

        //public bool Enabled { get; set; }

        //protected bool IsCellEnabled(int rowNumber)
        //{
        //    return true;
        //}

        protected override void  Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) { return; }//do not draw if not visable
            DataGrid dg = this.Owner;
            if (dg == null) { return; }

            g.FillRectangle(backBrush, bounds);
            this.PaintBoarder(g, bounds, rowNum);
            
            object obj = this.GetCellValue(rowNum);
            string cellText;
            if(!String.IsNullOrEmpty(this.Format) && obj is IFormattable)
            {
                cellText = ((IFormattable)obj).ToString(this.Format,System.Globalization.CultureInfo.CurrentCulture);
            }
            else
            {
                cellText = this.GetCellValue(rowNum).ToString();
            }

            bounds.Inflate(-3, -2);
            RectangleF textBounds = new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            g.DrawString(cellText, dg.Font, foreBrush, textBounds);
        }


        private void PaintBoarder(Graphics g, Rectangle bounds, int rowNum)
        {
            System.Diagnostics.Debug.Assert(!(this._mouseDown && ( this._mouseRowNum == -1)));

            EditableDataGrid grid = this.Owner as EditableDataGrid;
            if (grid == null) { return; }
            Pen boarderPen = grid.ForePen; 

            bool isClicked = (this._mouseDown && (rowNum == this._mouseRowNum));
            if (isClicked)
            {
                this.DrawBorderClicked(g, bounds, boarderPen);
            }
            else
            {
                this.DrawBorderNormal(g, bounds, boarderPen);               
            }
        }


        //protected string getCellText(int rowNum)
        //{
        //    String cellText;
        //    if(!UseCellColumnTextForCellValue)
        //    {
        //        cellText = GetCellValue(rowNum).ToString();
        //    }
        //    else
        //    {
        //        cellText = this.Text;
        //    }
        //    return cellText;
        //}

        public override object GetCellValue(int rowNum)
        {            
            if (string.IsNullOrEmpty(MappingName) || UseCellColumnTextForCellValue)
            {
                return this.Text;
            }
            else
            {
                CurrencyManager source = Owner.BindingContext[this.Owner.DataSource] as CurrencyManager;
                return this.PropertyDescriptor.GetValue(source.List[rowNum]);
            }
        }

        private void DrawBorderClicked(Graphics g, Rectangle bounds, Pen borderPen)
        {

            System.Diagnostics.Debug.Assert(borderPen != null);
            //top
            g.DrawLine(borderPen, bounds.X, bounds.Y + 1, bounds.X + bounds.Width - 1, bounds.Y +1);
            //left
            g.DrawLine(borderPen, bounds.X +1, bounds.Y, bounds.X+1, bounds.Y + bounds.Height - 1);
            ////botom
            //g.DrawLine(borderPen, bounds.X, bounds.Y + bounds.Height - 1, bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);
            ////right
            //g.DrawLine(borderPen, bounds.X + bounds.Width - 1, bounds.Y, bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);
        }

        private void DrawBorderNormal(Graphics g, Rectangle bounds, Pen borderPen)
        {
            System.Diagnostics.Debug.Assert(borderPen != null);
            ////draw top border shadowed
            //g.DrawLine(borderPen, bounds.X, bounds.Y + 1, bounds.X + bounds.Width - 1, bounds.Y + 1);
            ////draw left border shadowed
            //g.DrawLine(borderPen, bounds.X + 1, bounds.Y, bounds.X + 1, bounds.Y + bounds.Height - 1);
            //draw bottom border, highlighted
            g.DrawLine(borderPen, bounds.X, bounds.Y + bounds.Height - 1, bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);
            //draw right border, highlighted
            g.DrawLine(borderPen, bounds.X + bounds.Width - 1, bounds.Y, bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);



        }

        

        protected PropertyDescriptor CreateDefaultPropertyDescriptor()
        {
            PropertyDescriptor pd = TypeDescriptor.GetProperties(typeof(DataGridButtonColumn)).Find("Text", false);
            return pd;
        }


        //#region IClickableDataGridColumn Members

        public void HandleMouseDown(int rowNum, System.Windows.Forms.MouseEventArgs mea)
        {
            this._mouseDown = true;
            this._mouseRowNum = rowNum;

            //Graphics g = Owner.CreateGraphics();
            //try
            //{
            //    Rectangle cellBounds = this.Owner.GetCellBounds(rowNum, this.ColumnOrdinal);
            //    LayoutData layoutData = DoCellLayout(g, cellBounds, rowNum, true);
            //    this.PaintDown(g, Colors, layoutData);
            //}
            //catch
            //{ }//do nothing
        }

        public void HandleMouseUp(int rowNum, System.Windows.Forms.MouseEventArgs mea)
        {
            if (_mouseDown)
            {
                this._mouseDown = false;
                this._mouseRowNum = -1;
            }
        }


        public void HandleMouseClick(int rowNum)
        {
            if (_mouseDown)
            {
                this._mouseDown = false;
                this._mouseRowNum = -1;
            }

            if (this.Click != null)
            {
                this.Click(new ButtonCellClickEventArgs()
                {
                    CellValue = this.GetCellValue(rowNum),
                    DataGrid = this.Owner,
                    RowNumber = rowNum
                });
            }
        }
    }
}

