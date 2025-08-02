using System.Collections.ObjectModel;
using ReactiveUI;
using LogicSim.Core.Models.Gates;
using LogicSim.Core.Models;

namespace LogicSim.ViewModels;

public enum WiringState
{
    Idle,
    StartingWire,
    DraggingWire
}

public class CircuitCanvasViewModel : ViewModelBase
{
    private ObservableCollection<GateViewModel> _gates;
    private ObservableCollection<WireViewModel> _wires;
    private int _gateCounter = 0;
    private WiringState _wiringState = WiringState.Idle;
    private PinViewModel? _wireSourcePin;
    private double _previewEndX;
    private double _previewEndY;
    
    public CircuitCanvasViewModel()
    {
        _gates = new ObservableCollection<GateViewModel>();
        _wires = new ObservableCollection<WireViewModel>();
    }
    
    public ObservableCollection<GateViewModel> Gates
    {
        get => _gates;
        set => this.RaiseAndSetIfChanged(ref _gates, value);
    }
    
    public ObservableCollection<WireViewModel> Wires
    {
        get => _wires;
        set => this.RaiseAndSetIfChanged(ref _wires, value);
    }
    
    public WiringState WiringState
    {
        get => _wiringState;
        set
        {
            System.Diagnostics.Debug.WriteLine($"WiringState changed: {_wiringState} -> {value}");
            this.RaiseAndSetIfChanged(ref _wiringState, value);
        }
    }
    
    public PinViewModel? WireSourcePin
    {
        get => _wireSourcePin;
        set => this.RaiseAndSetIfChanged(ref _wireSourcePin, value);
    }
    
    public double PreviewEndX
    {
        get => _previewEndX;
        set => this.RaiseAndSetIfChanged(ref _previewEndX, value);
    }
    
    public double PreviewEndY
    {
        get => _previewEndY;
        set => this.RaiseAndSetIfChanged(ref _previewEndY, value);
    }
    
    public bool IsWiring => WiringState != WiringState.Idle;
    
    public void AddGate(GateType type, double x = 50, double y = 50)
    {
        var gate = new LogicGate(type, x, y);
        Gates.Add(new GateViewModel(gate));
    }
    
    public (double X, double Y) GetNextGatePosition()
    {
        const double startX = 250;
        const double startY = 50;
        const double gateWidth = 100;
        const double gateHeight = 70;
        const int gatesPerRow = 6;
        
        int row = _gateCounter / gatesPerRow;
        int col = _gateCounter % gatesPerRow;
        
        _gateCounter++;
        
        return (startX + col * gateWidth, startY + row * gateHeight);
    }
    
    public void StartWire(PinViewModel sourcePin)
    {
        if (WiringState == WiringState.Idle)
        {
            WireSourcePin = sourcePin;
            WiringState = WiringState.StartingWire;
            System.Diagnostics.Debug.WriteLine($"Started wire from pin: {sourcePin.Name} ({sourcePin.Direction})");
        }
    }
    
    public void UpdatePreviewLine(double x, double y)
    {
        if (WiringState == WiringState.StartingWire || WiringState == WiringState.DraggingWire)
        {
            PreviewEndX = x;
            PreviewEndY = y;
            if (WiringState == WiringState.StartingWire)
            {
                WiringState = WiringState.DraggingWire;
            }
        }
    }
    
    public void CompleteWire(PinViewModel targetPin)
    {
        if (WiringState != WiringState.Idle && WireSourcePin != null && WireSourcePin != targetPin)
        {
            // Find the gates that contain these pins
            var sourceGate = FindGateContainingPin(WireSourcePin);
            var targetGate = FindGateContainingPin(targetPin);
            
            if (sourceGate != null && targetGate != null)
            {
                var connection = new Connection(WireSourcePin.Pin.Id, targetPin.Pin.Id);
                var wireViewModel = new WireViewModel(connection, WireSourcePin, targetPin, sourceGate, targetGate);
                Wires.Add(wireViewModel);
                
                System.Diagnostics.Debug.WriteLine($"Wire created: {WireSourcePin.Name} -> {targetPin.Name}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Could not find gates for pins - wire creation failed");
            }
            
            CancelWire();
        }
    }
    
    public GateViewModel? FindGateContainingPin(PinViewModel pin)
    {
        return Gates.FirstOrDefault(gate => 
            gate.InputPins.Contains(pin) || gate.OutputPins.Contains(pin));
    }
    
    public void CancelWire()
    {
        if (WiringState != WiringState.Idle)
        {
            System.Diagnostics.Debug.WriteLine("Wire creation cancelled");
            WireSourcePin = null;
            WiringState = WiringState.Idle;
        }
    }
    
    public void RemoveWire(WireViewModel wire)
    {
        if (Wires.Contains(wire))
        {
            Wires.Remove(wire);
            System.Diagnostics.Debug.WriteLine($"Wire removed: {wire.StartPin?.Name} -> {wire.EndPin?.Name}");
        }
    }
}