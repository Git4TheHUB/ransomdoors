using System.Diagnostics;
using System.IO;
using System.Windows;

namespace rans0m;

public static class BrowserMode
{
    public static void Open()
    {
        string page = Path.Combine(AppContext.BaseDirectory, "Browser", "index.html");
        if (!File.Exists(page))
        {
            MessageBox.Show("Chrome mode is not included in this build.", "RANS0M", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string url = new Uri(page).AbsoluteUri;
        string? chrome = FindChrome();
        if (chrome is null)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = chrome,
            Arguments = $"--new-window \"{url}\"",
            UseShellExecute = false
        });
    }

    private static string? FindChrome()
    {
        string[] candidates =
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "Application", "chrome.exe")
        };

        return candidates.FirstOrDefault(File.Exists);
    }
}
