using System.Windows.Forms;

namespace EditableDataGridCF
{
    public interface IKeyPressProcessor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyVal"></param>
        /// <returns>true if the key press was handled</returns>
        bool ProcessKeyPress(Keys keyVal);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyVal"></param>
        /// <returns>true if the key press was handled</returns>
        bool ProcessDialogKey(Keys keyVal);

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if the key press was handled</returns>
        bool ProcessTabKey();

        bool ProcessBackTabKey();


        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if the key press was handled</returns>
        bool ProcessReturnKey();
    }
}
