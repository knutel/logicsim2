using ReactiveUI;

namespace LogicSim.ViewModels;

public class WireSegment : ViewModelBase
{
    private double _startX;
    private double _startY;
    private double _endX;
    private double _endY;
    
    public WireSegment(double startX, double startY, double endX, double endY)
    {
        _startX = startX;
        _startY = startY;
        _endX = endX;
        _endY = endY;
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
    
    public bool IsHorizontal => Math.Abs(_endY - _startY) < 0.1; // Allow small tolerance for floating point
    
    public bool IsVertical => Math.Abs(_endX - _startX) < 0.1;
    
    public double Length
    {
        get
        {
            if (IsHorizontal)
                return Math.Abs(_endX - _startX);
            if (IsVertical)
                return Math.Abs(_endY - _startY);
            return Math.Sqrt(Math.Pow(_endX - _startX, 2) + Math.Pow(_endY - _startY, 2));
        }
    }
    
    public void UpdatePoints(double startX, double startY, double endX, double endY)
    {
        StartX = startX;
        StartY = startY;
        EndX = endX;
        EndY = endY;
    }
}