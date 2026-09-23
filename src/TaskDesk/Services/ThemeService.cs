using Microsoft.Win32;

namespace TaskDesk.Services;

public sealed class ThemeService
{
    public bool IsDarkMode()
    {
        using var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
        var value = key?.GetValue("AppsUseLightTheme");
        return value is int setting && setting == 0;
    }

    public void Apply(System.Windows.Application application)
    {
        var dark = IsDarkMode();
        application.Resources["WindowBackgroundBrush"] = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(dark ? "#FF202020" : "#FFF7F7F7"));
        application.Resources["PanelBackgroundBrush"] = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(dark ? "#FF2B2B2B" : "#FFFFFFFF"));
        application.Resources["BorderBrush"] = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(dark ? "#FF454545" : "#FFD6D6D6"));
        application.Resources["ForegroundBrush"] = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(dark ? "#FFF2F2F2" : "#FF202020"));
    }
}
