using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PS2Home.App.Views;

public partial class DiagnosticsView : UserControl
{
    public DiagnosticsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
