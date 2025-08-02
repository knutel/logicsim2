using ReactiveUI;
using LogicSim.Core.Models.Gates;
using LogicSim.Core.Utilities;
using System.Collections.ObjectModel;

namespace LogicSim.ViewModels;

public class GateViewModel : ViewModelBase
{
    private readonly LogicGate _gate;
    private double _x;
    private double _y;
    private bool _isDragging;
    private double _dragOffsetX;
    private double _dragOffsetY;
    private ObservableCollection<PinViewModel> _inputPins;
    private ObservableCollection<PinViewModel> _outputPins;
    
    public GateViewModel(LogicGate gate)
    {
        _gate = gate;
        _x = gate.X;
        _y = gate.Y;
        
        // Initialize pin collections
        _inputPins = new ObservableCollection<PinViewModel>(
            gate.InputPins.Select(pin => new PinViewModel(pin))
        );
        
        _outputPins = new ObservableCollection<PinViewModel>(
            gate.OutputPins.Select(pin => new PinViewModel(pin))
        );
        
        // Debug output
        System.Diagnostics.Debug.WriteLine($"GateViewModel created for {gate.Type}: InputPins={InputPins.Count}, OutputPins={OutputPins.Count}");
        
        UpdatePinPositions();
    }
    
    public LogicGate Gate => _gate;
    
    public GateType Type => _gate.Type;
    
    public string Name => _gate.Name;
    
    public ObservableCollection<PinViewModel> InputPins
    {
        get => _inputPins;
        set => this.RaiseAndSetIfChanged(ref _inputPins, value);
    }
    
    public ObservableCollection<PinViewModel> OutputPins
    {
        get => _outputPins;
        set => this.RaiseAndSetIfChanged(ref _outputPins, value);
    }
    
    public double X
    {
        get => _x;
        set
        {
            this.RaiseAndSetIfChanged(ref _x, value);
            _gate.X = value;
            UpdatePinPositions();
        }
    }
    
    public double Y
    {
        get => _y;
        set
        {
            this.RaiseAndSetIfChanged(ref _y, value);
            _gate.Y = value;
            UpdatePinPositions();
        }
    }
    
    public bool IsDragging
    {
        get => _isDragging;
        set => this.RaiseAndSetIfChanged(ref _isDragging, value);
    }
    
    public double DragOffsetX
    {
        get => _dragOffsetX;
        set => this.RaiseAndSetIfChanged(ref _dragOffsetX, value);
    }
    
    public double DragOffsetY
    {
        get => _dragOffsetY;
        set => this.RaiseAndSetIfChanged(ref _dragOffsetY, value);
    }
    
    public void StartDrag(double pointerX, double pointerY)
    {
        IsDragging = true;
        DragOffsetX = pointerX - X;
        DragOffsetY = pointerY - Y;
    }
    
    public void UpdatePosition(double pointerX, double pointerY)
    {
        if (IsDragging)
        {
            // Update position during drag without snapping for smooth movement
            X = pointerX - DragOffsetX;
            Y = pointerY - DragOffsetY;
        }
    }
    
    public void EndDrag()
    {
        if (IsDragging)
        {
            // Snap to grid when drag ends
            var snappedPosition = GridHelper.SnapPoint(X, Y);
            X = snappedPosition.X;
            Y = snappedPosition.Y;
            
            IsDragging = false;
        }
    }
    
    private void UpdatePinPositions()
    {
        _gate.UpdatePinPositions();
        
        // Update pin ViewModels with new positions
        for (int i = 0; i < InputPins.Count; i++)
        {
            var pinVm = InputPins[i];
            var pin = _gate.InputPins[i];
            pinVm.X = pin.X;
            pinVm.Y = pin.Y;
        }
        
        for (int i = 0; i < OutputPins.Count; i++)
        {
            var pinVm = OutputPins[i];
            var pin = _gate.OutputPins[i];
            pinVm.X = pin.X;
            pinVm.Y = pin.Y;
        }
    }
}