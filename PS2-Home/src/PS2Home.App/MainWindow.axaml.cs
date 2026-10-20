using Avalonia.Controls;
using PS2Home.Core;

namespace PS2Home.App;

public partial class MainWindow : Window
{
    private static readonly (string Title, string Body)[] Pages =
    {
        ("Library", "Your games will appear here. (Placeholder - Phase 2 adds real scanning.)"),
        ("Settings", "PCSX2 location and preferences will live here. (Placeholder - Phase 3.)"),
        ("Diagnostics", "Setup checks and logs will appear here. (Placeholder.)"),
    };

    public MainWindow()
    {
        InitializeComponent();
        TaglineText.Text = AppInfo.Tagline;
        ShowPage(0);
    }

    private void OnNavSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (NavList.SelectedIndex >= 0)
            ShowPage(NavList.SelectedIndex);
    }

    private void ShowPage(int index)
    {
        PageTitle.Text = Pages[index].Title;
        PageBody.Text = Pages[index].Body;
    }
}
