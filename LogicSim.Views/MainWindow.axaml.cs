using Avalonia.ReactiveUI;
using LogicSim.ViewModels;

namespace LogicSim.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}