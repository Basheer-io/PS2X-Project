using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using PS2Home.App.ViewModels;

namespace PS2Home.App.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void OnExportBackupClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsViewModel vm) return;
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export PS2 Home Backup",
            SuggestedFileName = $"ps2home-backup-{DateTime.UtcNow:yyyyMMdd}.json",
            DefaultExtension = "json",
            FileTypeChoices =
            [
                new FilePickerFileType("JSON Backup Files (*.json)")
                {
                    Patterns = ["*.json"]
                }
            ]
        });

        if (file != null)
        {
            await vm.ExportBackupToPathAsync(file.Path.LocalPath);
        }
    }

    private async void OnImportBackupClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsViewModel vm) return;
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select PS2 Home Backup to Restore",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("JSON Backup Files (*.json)")
                {
                    Patterns = ["*.json"]
                }
            ]
        });

        if (files.Count > 0)
        {
            await vm.RestoreBackupFromPathAsync(files[0].Path.LocalPath);
        }
    }
}
