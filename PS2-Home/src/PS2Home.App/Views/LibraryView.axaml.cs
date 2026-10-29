using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PS2Home.App.ViewModels;

namespace PS2Home.App.Views;

public partial class LibraryView : UserControl
{
    public LibraryView()
    {
        InitializeComponent();
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not LibraryViewModel vm)
            return;

        if (e.Key == Key.Escape)
        {
            if (vm.HasSearchQuery)
            {
                vm.ClearSearch();
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Enter)
        {
            // If user is focused on the search box, do not intercept Enter
            if (e.Source is TextBox)
                return;

            if (vm.SelectedApp != null && vm.SelectedApp.CanLaunch)
            {
                _ = vm.LaunchAppAsync(vm.SelectedApp);
                e.Handled = true;
            }
        }
    }

    private async void OnSetCoverClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is AppItemViewModel app && DataContext is LibraryViewModel vm)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = $"Select Cover Art for {app.Name}",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Images (*.png, *.jpg, *.jpeg, *.webp, *.bmp)")
                    {
                        Patterns = ["*.png", "*.jpg", "*.jpeg", "*.webp", "*.bmp"]
                    }
                ]
            });

            if (files.Count > 0)
            {
                var localPath = files[0].Path.LocalPath;
                await vm.SetCoverArtAsync(app, localPath);
            }
        }
    }

    private async void OnRemoveCoverClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is AppItemViewModel app && DataContext is LibraryViewModel vm)
        {
            await vm.RemoveCoverArtAsync(app);
        }
    }
}
