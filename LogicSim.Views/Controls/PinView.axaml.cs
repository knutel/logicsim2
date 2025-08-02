using Avalonia.Controls;
using Avalonia.Input;
using LogicSim.ViewModels;

namespace LogicSim.Views.Controls;

public partial class PinView : UserControl
{
    public event EventHandler<PinClickedEventArgs>? PinClicked;
    
    public PinView()
    {
        InitializeComponent();
        PinEllipse.PointerPressed += OnPinPressed;
    }
    
    private void OnPinPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is PinViewModel pinViewModel)
        {
            System.Diagnostics.Debug.WriteLine($"Pin clicked: {pinViewModel.Name} ({pinViewModel.Direction})");
            PinClicked?.Invoke(this, new PinClickedEventArgs(pinViewModel));
            e.Handled = true;
        }
    }
}

public class PinClickedEventArgs : EventArgs
{
    public PinViewModel PinViewModel { get; }
    
    public PinClickedEventArgs(PinViewModel pinViewModel)
    {
        PinViewModel = pinViewModel;
    }
}