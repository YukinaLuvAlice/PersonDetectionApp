using System;
using System.Runtime.InteropServices;

namespace PersonDetectionApp.Services
{
    public class ScreenService
    {
        // P/Invoke để điều khiển màn hình
        [DllImport("user32.dll")]
        private static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);

        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MONITORPOWER = 0xF170;

        // Trạng thái màn hình
        private const int MONITOR_ON = -1;
        private const int MONITOR_OFF = 2;

        public void TurnScreenOn()
        {
            // Khôi phục hệ thống từ trạng thái tiết kiệm năng lượng
            SendMessage(0xFFFF, WM_SYSCOMMAND, SC_MONITORPOWER, MONITOR_ON);
        }

        public void TurnScreenOff()
        {
            // Đặt hệ thống vào trạng thái tiết kiệm năng lượng
            SendMessage(0xFFFF, WM_SYSCOMMAND, SC_MONITORPOWER, MONITOR_OFF);
        }
    }
}