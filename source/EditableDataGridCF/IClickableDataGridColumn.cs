using System.Drawing;

namespace EditableDataGridCF
{
    interface IClickableDataGridColumn
    {
        void HandleMouseDown(int rowNum, System.Windows.Forms.MouseEventArgs mea);

        void HandleMouseUp(int rowNum, System.Windows.Forms.MouseEventArgs mea);

        void HandleMouseClick(int rowNum);
    }
}
