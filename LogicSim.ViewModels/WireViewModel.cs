using ReactiveUI;
using LogicSim.Core.Models;

namespace LogicSim.ViewModels;

public class WireViewModel : ViewModelBase
{
    private readonly Connection _connection;
    private double _startX;
    private double _startY;
    private double _endX;
    private double _endY;
    private PinViewModel? _startPin;
    private PinViewModel? _endPin;
    private GateViewModel? _startGate;
    private GateViewModel? _endGate;
    
    public WireViewModel(Connection connection, PinViewModel startPin, PinViewModel endPin, GateViewModel startGate, GateViewModel endGate)
    {
        _connection = connection;
        _startPin = startPin;
        _endPin = endPin;
        _startGate = startGate;
        _endGate = endGate;
        
        UpdatePositions();
        
        // Subscribe to pin position changes
        _startPin.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(PinViewModel.X) || e.PropertyName == nameof(PinViewModel.Y))
                UpdatePositions();
        };
        
        _endPin.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(PinViewModel.X) || e.PropertyName == nameof(PinViewModel.Y))
                UpdatePositions();
        };
        
        // Subscribe to gate position changes
        _startGate.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(GateViewModel.X) || e.PropertyName == nameof(GateViewModel.Y))
                UpdatePositions();
        };
        
        _endGate.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(GateViewModel.X) || e.PropertyName == nameof(GateViewModel.Y))
                UpdatePositions();
        };
        
        System.Diagnostics.Debug.WriteLine($"WireViewModel created: {startPin.Name} -> {endPin.Name}");
    }
    
    public Connection Connection => _connection;
    
    public PinViewModel? StartPin
    {
        get => _startPin;
        set => this.RaiseAndSetIfChanged(ref _startPin, value);
    }
    
    public PinViewModel? EndPin
    {
        get => _endPin;
        set => this.RaiseAndSetIfChanged(ref _endPin, value);
    }
    
    public double StartX
    {
        get => _startX;
        set => this.RaiseAndSetIfChanged(ref _startX, value);
    }
    
    public double StartY
    {
        get => _startY;
        set => this.RaiseAndSetIfChanged(ref _startY, value);
    }
    
    public double EndX
    {
        get => _endX;
        set => this.RaiseAndSetIfChanged(ref _endX, value);
    }
    
    public double EndY
    {
        get => _endY;
        set => this.RaiseAndSetIfChanged(ref _endY, value);
    }
    
    public string DisplayColor => "#4F5D75"; // Wire color matching theme
    
    private void UpdatePositions()
    {
        if (_startPin != null && _endPin != null && _startGate != null && _endGate != null)
        {
            // Calculate absolute positions: gate position + pin relative position + pin center offset
            StartX = _startGate.X + _startPin.RelativeX + 6; // Center of 12px pin
            StartY = _startGate.Y + _startPin.RelativeY + 6;
            EndX = _endGate.X + _endPin.RelativeX + 6;
            EndY = _endGate.Y + _endPin.RelativeY + 6;
        }
    }
}