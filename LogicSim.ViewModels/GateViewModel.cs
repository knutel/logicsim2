using ReactiveUI;
using LogicSim.Core.Models.Gates;

namespace LogicSim.ViewModels;

public class GateViewModel : ViewModelBase
{
    private readonly LogicGate _gate;
    private double _x;
    private double _y;
    private bool _isDragging;
    private double _dragOffsetX;
    private double _dragOffsetY;
    
    public GateViewModel(LogicGate gate)
    {
        _gate = gate;
        _x = gate.X;
        _y = gate.Y;
    }
    
    public LogicGate Gate => _gate;
    
    public GateType Type => _gate.Type;
    
    public string Name => _gate.Name;
    
    public double X
    {
        get => _x;
        set
        {
            this.RaiseAndSetIfChanged(ref _x, value);
            _gate.X = value;
        }
    }
    
    public double Y
    {
        get => _y;
        set
        {
            this.RaiseAndSetIfChanged(ref _y, value);
            _gate.Y = value;
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
            X = pointerX - DragOffsetX;
            Y = pointerY - DragOffsetY;
        }
    }
    
    public void EndDrag()
    {
        IsDragging = false;
    }
}