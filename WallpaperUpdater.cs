using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using Microsoft.Win32;

namespace rans0m
{
    internal class WallpaperUpdater
    {
        private static string originalWallpaper;

        private const uint SPI_SETDESKWALLPAPER = 20;
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDCHANGE = 0x02;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(uint uAction, uint uParam, string lpvParam, uint fuWinIni);

        /// <summary>
        /// Change desktop background to dark red
        /// </summary>
        static public void SetDarkRedWallpaper()
        {
            Task.Run(() =>
            {
                try
                {
                    // Save original wallpaper path
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
                    {
                        originalWallpaper = key?.GetValue("Wallpaper")?.ToString();
                    }

                    // Create temp dark red image, I should probably store it in ressource instead
                    string tempPath = Path.Combine(Path.GetTempPath(), "darkred_wallpaper.bmp");
                    using (Bitmap bmp = new Bitmap(1920, 1080))
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.Clear(Color.FromArgb(50, 0, 0));
                        }
                        bmp.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);
                    }

                    // Set wallpaper
                    SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                }
                catch { }
            });
        }

        /// <summary>
        /// Restore original wallpaper
        /// </summary>
        static public void RestoreWallpaper()
        {
            try
            {
                if (!string.IsNullOrEmpty(originalWallpaper))
                {
                    SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, originalWallpaper, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                }

                // Cleanup temp file
                string tempPath = Path.Combine(Path.GetTempPath(), "darkred_wallpaper.bmp");
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
            catch { }
        }
    }
}
