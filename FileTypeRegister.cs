using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace rans0m
{
    public static class FileTypeRegister
    {
        private const string ClassesRoot = @"Software\Classes";

        private static readonly string[] GoldExtensions = { ".gold1", ".gold2", ".gold3", ".gold4", ".gold5", ".gold6", ".crucifix" };
        private static readonly string[] GoldFileTypeNames = { "GoldFile1", "GoldFile2", "GoldFile3", "GoldFile4", "GoldFile5", "GoldFile6", "CrucifixFile" };

        // ---------------- PUBLIC METHODS ----------------

        /// <summary>
        /// Registers a file type with the specified extension and icon.
        /// </summary>
        public static void RegisterIconForExtension(string extension, byte[] iconData, string fileTypeName)
        {
            using MemoryStream ms = new MemoryStream(iconData);
            Icon icon = new Icon(ms);

            using (icon)
            {
                string iconPath = SaveIconToDisk(icon, extension.TrimStart('.'));

                // Maps the extension to fileTypeName
                using (RegistryKey extKey = Registry.CurrentUser.CreateSubKey(Combine(ClassesRoot, extension)))
                { extKey.SetValue("", fileTypeName); }

                // Links the .ico file to the fileTypeName
                using (RegistryKey defaultIconKey = Registry.CurrentUser.CreateSubKey(Combine(ClassesRoot, fileTypeName, "DefaultIcon")))
                { defaultIconKey.SetValue("", iconPath); }

                NotifyShellOfChange();
            }
        }

        /// <summary>
        /// Unregisters a file type with the specified extension and icon.
        /// </summary>
        public static void UnregisterFileType(string extension, string fileTypeName)
        {
            Registry.CurrentUser.DeleteSubKeyTree(Combine(ClassesRoot, extension), throwOnMissingSubKey: false);
            Registry.CurrentUser.DeleteSubKeyTree(Combine(ClassesRoot, fileTypeName), throwOnMissingSubKey: false);
            NotifyShellOfChange();
        }

        /// <summary>
        /// Unregisters every .gold1-6/.crucifix file type and removes the icon files/folder written to disk for them.
        /// </summary>
        public static void UnregisterAllGoldFileTypes()
        {
            for (int i = 0; i < GoldExtensions.Length; i++)
            {
                UnregisterFileType(GoldExtensions[i], GoldFileTypeNames[i]);
            }

            try
            {
                string iconFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RANSOM");
                if (Directory.Exists(iconFolder))
                    Directory.Delete(iconFolder, true);
            }
            catch { }
        }



        // ------------------ PRIVATE METHODS ------------------

        private static string SaveIconToDisk(Icon icon, string extensionName)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RANSOM", "Icons");
            Directory.CreateDirectory(folder);

            string iconPath = Path.Combine(folder, extensionName + ".ico");
            using FileStream fs = new FileStream(iconPath, FileMode.Create, FileAccess.Write);
            icon.Save(fs);
            return iconPath;
        }

        private static string Combine(params string[] parts)
        {
            return string.Join("\\", Array.FindAll(parts, p => !string.IsNullOrEmpty(p)));
        }

        private static void NotifyShellOfChange()
        {
            const uint SHCNE_ASSOCCHANGED = 0x08000000;
            const uint SHCNF_IDLIST = 0x0000;
            NativeMethods.SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        private static class NativeMethods
        {
            [DllImport("shell32.dll")]
            public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
        }
    }
}