using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Veylib.WinForms
{
    public class Movement
    {
        #region Setup window dragging
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        #endregion

        public IntPtr FormHandle;

        public void MouseDownMove(MouseEventArgs e)
        {
            if (FormHandle == null)
                throw new Exception("Form handle intptr null, cannot ");

            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(FormHandle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}
