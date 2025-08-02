using ReactiveUI;
using LogicSim.Core.Models;

namespace LogicSim.ViewModels;

public class PinViewModel : ViewModelBase
{
    private readonly Pin _pin;
    private double _x;
    private double _y;
    private bool _isHovered;
    private bool _isConnected;
    private bool _isWireSource;
    private bool _isWireTarget;
    
    public PinViewModel(Pin pin)
    {
        _pin = pin;
        _x = pin.X;
        _y = pin.Y;
        _isConnected = pin.IsConnected;
    }
    
    public Pin Pin => _pin;
    
    public PinDirection Direction => _pin.Direction;
    
    public string Name => _pin.Name;
    
    public double X
    {
        get => _x;
        set
        {
            this.RaiseAndSetIfChanged(ref _x, value);
            _pin.X = value;
            this.RaisePropertyChanged(nameof(RelativeX));
        }
    }
    
    public double Y
    {
        get => _y;
        set
        {
            this.RaiseAndSetIfChanged(ref _y, value);
            _pin.Y = value;
            this.RaisePropertyChanged(nameof(RelativeY));
        }
    }
    
    // Relative coordinates for positioning within GateView
    public double RelativeX => X + 10; // Offset by gate body position (Canvas.Left="10")
    public double RelativeY => Y + 5;  // Offset by gate body position (Canvas.Top="5")
    
    public bool IsHovered
    {
        get => _isHovered;
        set => this.RaiseAndSetIfChanged(ref _isHovered, value);
    }
    
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            this.RaiseAndSetIfChanged(ref _isConnected, value);
            _pin.IsConnected = value;
            this.RaisePropertyChanged(nameof(DisplayColor));
        }
    }
    
    public bool IsWireSource
    {
        get => _isWireSource;
        set
        {
            this.RaiseAndSetIfChanged(ref _isWireSource, value);
            this.RaisePropertyChanged(nameof(DisplayColor));
        }
    }
    
    public bool IsWireTarget
    {
        get => _isWireTarget;
        set
        {
            this.RaiseAndSetIfChanged(ref _isWireTarget, value);
            this.RaisePropertyChanged(nameof(DisplayColor));
        }
    }
    
    public string DisplayColor
    {
        get
        {
            if (IsWireSource) return "#FFD700"; // Gold for wire source
            if (IsWireTarget) return "#32CD32"; // Lime green for wire target
            
            return Direction == PinDirection.Input 
                ? "#00FF00"  // Bright green for input pins (debug)
                : "#FF0000"; // Bright red for output pins (debug)
        }
    }
    
    public string HoverColor => Direction == PinDirection.Input ? "#3A7BC8" : "#C23A3A";
}