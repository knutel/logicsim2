using ReactiveUI;

namespace LogicSim.ViewModels;

public class BendPoint : ViewModelBase
{
    private double _x;
    private double _y;
    private bool _isDragging;
    private bool _isHovered;
    
    public BendPoint(double x, double y)
    {
        _x = x;
        _y = y;
    }
    
    public double X
    {
        get => _x;
        set => this.RaiseAndSetIfChanged(ref _x, value);
    }
    
    public double Y
    {
        get => _y;
        set => this.RaiseAndSetIfChanged(ref _y, value);
    }
    
    public bool IsDragging
    {
        get => _isDragging;
        set
        {
            this.RaiseAndSetIfChanged(ref _isDragging, value);
            this.RaisePropertyChanged(nameof(DisplayColor));
        }
    }
    
    public bool IsHovered
    {
        get => _isHovered;
        set
        {
            this.RaiseAndSetIfChanged(ref _isHovered, value);
            this.RaisePropertyChanged(nameof(DisplayColor));
        }
    }
    
    public string DisplayColor
    {
        get
        {
            if (IsDragging) return "#FF6B6B"; // Red when dragging
            if (IsHovered) return "#4ECDC4"; // Teal when hovered
            return "#95A5A6"; // Gray default
        }
    }
    
    public void MoveTo(double x, double y)
    {
        X = x;
        Y = y;
    }
}