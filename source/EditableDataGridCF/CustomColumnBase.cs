using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace EditableDataGridCF
{
    public abstract partial class CustomColumnBase : DataGridTextBoxColumn
    {
        #region Statics
        private static System.Reflection.FieldInfo ownerAccessor;                                          //FieldInfo for retrieving owner value

        /// <summary>
        /// Static Constructor. Called automaticly
        /// </summary>
        static CustomColumnBase()
        {
            ownerAccessor = typeof(CustomColumnBase).GetField("m_dgOwner", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        #endregion Statics

        #region Privates

        private StringFormat _stringFormat = null;                                      // Actual string format we'll use to draw string.

        private int _columnOrdinal = -1;                                                // Our ordinal in the grid.

        #endregion Privates

        #region Public Properties

        public virtual bool ReadOnly
        {
            get
            {
                return true;
            }
            set
            {
                return;
            }
        }

        public virtual object NullValue
        {
            get
            {
                return this.NullText;
            }
            set
            {
                this.NullText = value.ToString();
            }
        }

        public StringFormat StringFormat
        {
            get
            {
                if (null == _stringFormat)                                              // No format yet?
                {
                    _stringFormat = new StringFormat();                                 // Create one.

                    this.Alignment = HorizontalAlignment.Left;                          // And set default aligment.
                }

                return _stringFormat;                                                   // Return our format
            }
        }

        public int ColumnOrdinal
        {
            get
            {
                if ((_columnOrdinal == -1) && (this.Owner != null))                     // Parent is set but ordinal is not?
                {
                    foreach (DataGridTableStyle table in this.Owner.TableStyles)        // Check all tables.
                    {
                        this._columnOrdinal = table.GridColumnStyles.IndexOf(this);     // Get our index.

                        if (this._columnOrdinal != -1) break;                           // Exit if found.
                    }
                }

                return _columnOrdinal;
            }
        }

        /// <summary>
        /// Gets the data grid we are a part of.
        /// </summary>
        public DataGrid Owner
        {
            get
            {
                return (DataGrid)ownerAccessor.GetValue(this);
            }
        }

        public virtual HorizontalAlignment Alignment
        {
            get
            {
                return (this.StringFormat.Alignment == StringAlignment.Center) ? HorizontalAlignment.Center :
                       (this.StringFormat.Alignment == StringAlignment.Far) ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            }
            set
            {
                if (this.Alignment != value)                                    // New aligment?
                {
                    this.StringFormat.Alignment = (value == HorizontalAlignment.Center) ? StringAlignment.Center :
                                                  (value == HorizontalAlignment.Right) ? StringAlignment.Far : StringAlignment.Near;
                    // Set it.
                    Invalidate();                                               // Aligment just changed, repaint.
                }
            }
        }

        #endregion Public Properties

        public virtual object GetCellValue(int rowNum)
        {
            CurrencyManager source = Owner.BindingContext[this.Owner.DataSource] as CurrencyManager;
            return this.GetCellValue(source, rowNum);
        }

        internal virtual object GetCellValue(CurrencyManager source, int rowNum)
        {
            if (source != null)
            {
                return this.PropertyDescriptor.GetValue(source.List[rowNum]);
            }
            return null;
        }

        public virtual void SetCellValue(int rowNum, object value)
        {
            CurrencyManager source = Owner.BindingContext[this.Owner.DataSource] as CurrencyManager;
            this.SetCellValue(source, rowNum, value);
        }

        internal virtual void SetCellValue(CurrencyManager source, int rowNum, object value)
        {
            if (source != null)
            {
                this.PropertyDescriptor.SetValue(source.List[rowNum], value);
            }
        }

        #region protected methods

        //private int _autoSizeWidth = -1;
        //private DataGridAutoSizeMode _autoSizeMode = DataGridAutoSizeMode.NotSet;
        //public DataGridAutoSizeMode AutoSizeMode
        //{
        //    get { return _autoSizeMode; }
        //    set
        //    {
        //        if (_autoSizeMode == value) { return; }
        //        _autoSizeMode = value;
        //        OnAutoSizeModeChanged();

        //    }

        //}

        //public override int Width
        //{
        //    get
        //    {
        //        return this.GetWidth();
        //    }
        //    set
        //    {
        //        base.Width = value;
        //    }
        //}

        //protected void OnAutoSizeModeChanged()
        //{
        //    _autoSizeWidth = -1;
        //}

        //protected int GetAutoSizeWidth()
        //{
        //    if (this.Owner == null || _autoSizeWidth > 0)
        //    {
        //        return _autoSizeWidth;
        //    }

        //    switch (this.AutoSizeMode)
        //    {
        //        case DataGridAutoSizeMode.AllCells:
        //            {
        //                CurrencyManager source = Owner.BindingContext[this.Owner.DataSource] as CurrencyManager;
        //                if (source != null)
        //                {
        //                    int maxW = 0;
        //                    foreach (object rowVal in source.List)
        //                    {
        //                        int w = this.MeasureText(this.PropertyDescriptor.GetValue(rowVal).ToString()).Width;
        //                        if (w > maxW)
        //                        {
        //                            maxW = w;
        //                        }
        //                    }
        //                    _autoSizeWidth = maxW;
        //                }
        //                else
        //                {
        //                    _autoSizeWidth = 0;
        //                }
        //                return _autoSizeWidth;
        //            }
        //        case DataGridAutoSizeMode.AllCellsExceptHeader:
        //        case DataGridAutoSizeMode.ColumnHeader:
        //            {
        //                _autoSizeWidth = this.MeasureText(this.HeaderText).Width;
        //                return _autoSizeWidth;
        //            }
        //        case DataGridAutoSizeMode.None:
        //        case DataGridAutoSizeMode.NotSet:
        //        default:
        //            {
        //                return base.Width;
        //            }
        //    }

        //}

        //protected int GetWidth()
        //{
        //    switch (this.AutoSizeMode)
        //    {
        //        case DataGridAutoSizeMode.AllCells:
        //        case DataGridAutoSizeMode.AllCellsExceptHeader:
        //        case DataGridAutoSizeMode.ColumnHeader:
        //            {
        //                return GetAutoSizeWidth();
        //            }
        //        case DataGridAutoSizeMode.None:
        //        case DataGridAutoSizeMode.NotSet:
        //        default:
        //            {
        //                return base.Width;
        //            }
        //    }
        //}

        //private Size MeasureText(Control c, string text)
        //{
        //    if (c == null)
        //    { return new Size(-1, -1); }
        //    using (Graphics g = c.CreateGraphics())
        //    {
        //        return g.MeasureString(text, this.Owner.Font).ToSize();
        //    }
        //}

        protected virtual String FormatText(Object cellData)
        {
            String cellText;                                                    // Formatted text.

            if ((null == cellData) || (DBNull.Value == cellData))               // See if data is null
            {
                cellText = this.NullText;                                       // It's null, so set it to NullText.
            }
            else if (cellData is IFormattable)                                  // Is data IFormattable?
            {
                cellText = ((IFormattable)cellData).ToString(this.Format, this.FormatInfo);
                // Yes, format it.
            }
            else if (cellData is IConvertible)                                  // May be it's IConvertible?
            {
                cellText = ((IConvertible)cellData).ToString(this.FormatInfo);  // We'll take that, no problem.
            }
            else
            {
                cellText = cellData.ToString();                                 // At this point we'll give up and simply call ToString()
            }

            return cellText;
        }

        protected static Rectangle AlignContentWithinCell(Size alignThis, Rectangle withinThis, ContentAlignment align)
        {
            //allign content horizontaly
            if ((align & ContentAlignment.TopRight) == ContentAlignment.TopRight)
                withinThis.X += withinThis.Width - alignThis.Width;
            else if ((align & ContentAlignment.TopCenter) == ContentAlignment.TopCenter)
                withinThis.X += (withinThis.Width - alignThis.Width) / 2;
            //else

            //do nothing content already right alligned

            //allign content verticly
            //alignThis.Height = withinThis.Height;

            return withinThis;
        }

        protected void Invalidate()
        {
            if (this.Owner != null)                                             // Got parent?
            {
                this.Owner.Invalidate();                                        // Repaint it.
            }
        }

        #endregion protected methods
    }
}