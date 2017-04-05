using System;

using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace EditableDataGridCF
{
    internal static class ControlExtentionMethods
    {
        public static bool InDesignMode(this Control @this)
        {
            return !(@this.Site == null) && @this.Site.DesignMode;
        }

        public static int MeasureTextWidth(this Control @this, string text)
        {
            if (@this == null)
            { return -1; }
            using (System.Drawing.Graphics g = @this.CreateGraphics())
            {
                return (int)Math.Ceiling(g.MeasureString(text, @this.Font).Width);
            }
        }
    }
}
