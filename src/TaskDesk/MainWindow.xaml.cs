using System.Windows;
using TaskDesk.ViewModels;

namespace TaskDesk;

public partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.HasResourceLibrary)
        {
            SelectResourceLibrary_Click(this, new RoutedEventArgs());
        }
    }

    private void SelectResourceLibrary_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "请选择资源库根目录。",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false
        };
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            try
            {
                ViewModel.SelectResourceLibrary(dialog.SelectedPath);
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show(this, exception.Message, "资源库选择失败", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }

    private void ResourceList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => ViewModel.OpenSelectedResource();

    private void SortTasks_Click(object sender, RoutedEventArgs e) => ViewModel.SortTasks();

    private void DeleteTask_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedTask is null)
        {
            return;
        }

        var result = System.Windows.MessageBox.Show(this, $"确认删除任务“{ViewModel.SelectedTask.Title}”？此操作只删除任务条目，不删除资源库文件。", "确认删除", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
        if (result == System.Windows.MessageBoxResult.Yes)
        {
            ViewModel.DeleteTaskCommand.Execute(null);
        }
    }

    private void RenameTask_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedTask is null)
        {
            return;
        }

        var input = new System.Windows.Controls.TextBox { Text = ViewModel.SelectedTask.Title, MinWidth = 300, Margin = new Thickness(12) };
        var dialog = new Window
        {
            Owner = this,
            Title = "重命名任务",
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new System.Windows.Controls.StackPanel
            {
                Children =
                {
                    input,
                    new System.Windows.Controls.Button { Content = "保存", IsDefault = true, Margin = new Thickness(12, 0, 12, 12), Padding = new Thickness(18, 6, 18, 6) }
                }
            }
        };
        ((System.Windows.Controls.Button)((System.Windows.Controls.StackPanel)dialog.Content).Children[1]).Click += (_, _) => dialog.DialogResult = true;
        if (dialog.ShowDialog() == true)
        {
            ViewModel.RenameSelectedTask(input.Text);
        }
    }

    private void More_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.MessageBox.Show(this, "TaskDesk 首版仅对任务数据提供编辑，对资源库提供只读浏览和 TXT/MD 预览。", "TaskDesk", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }
}