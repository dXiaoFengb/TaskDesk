using TaskDesk.Services;

namespace TaskDesk;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        new ThemeService().Apply(this);
        base.OnStartup(e);
    }
}