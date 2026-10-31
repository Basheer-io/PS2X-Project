using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PS2Home.App.ViewModels;

namespace PS2Home.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Source is TextBox)
            return;

        if (DataContext is not MainViewModel vm)
            return;

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.L)
        {
            vm.NavigateToLibraryCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.O)
        {
            vm.NavigateToSettingsCommand.Execute(null);
            e.Handled = true;
        }
    }
}
