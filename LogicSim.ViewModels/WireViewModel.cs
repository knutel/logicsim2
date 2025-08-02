using ReactiveUI;
using LogicSim.Core.Models;
using LogicSim.Core.Utilities;
using System.Collections.ObjectModel;

namespace LogicSim.ViewModels;

public enum WireRoutingPattern
{
    HVH, // Horizontal-Vertical-Horizontal
    VHV  // Vertical-Horizontal-Vertical
}

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
    private ObservableCollection<WireSegment> _segments;
    private ObservableCollection<BendPoint> _bendPoints;
    private WireRoutingPattern _routingPattern;
    private bool _isInitialRouting = true;
    
    public WireViewModel(Connection connection, PinViewModel startPin, PinViewModel endPin, GateViewModel startGate, GateViewModel endGate)
    {
        _connection = connection;
        _startPin = startPin;
        _endPin = endPin;
        _startGate = startGate;
        _endGate = endGate;
        _segments = new ObservableCollection<WireSegment>();
        _bendPoints = new ObservableCollection<BendPoint>();
        
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
        
        // Subscribe to bend point changes
        _bendPoints.CollectionChanged += (s, e) => RecalculateSegments();
        
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
    
    public ObservableCollection<WireSegment> Segments
    {
        get => _segments;
        set => this.RaiseAndSetIfChanged(ref _segments, value);
    }
    
    public ObservableCollection<BendPoint> BendPoints
    {
        get => _bendPoints;
        set => this.RaiseAndSetIfChanged(ref _bendPoints, value);
    }
    
    public string DisplayColor => "#4F5D75"; // Wire color matching theme
    
    public WireRoutingPattern RoutingPattern => _routingPattern;
    
    private void UpdatePositions()
    {
        if (_startPin != null && _endPin != null && _startGate != null && _endGate != null)
        {
            // Calculate absolute positions: gate position + pin relative position + pin center offset
            StartX = _startGate.X + _startPin.RelativeX + 6; // Center of 12px pin
            StartY = _startGate.Y + _startPin.RelativeY + 6;
            EndX = _endGate.X + _endPin.RelativeX + 6;
            EndY = _endGate.Y + _endPin.RelativeY + 6;
            
            if (_isInitialRouting)
            {
                // Only calculate routing on initial wire creation
                CalculateRouting();
                _isInitialRouting = false;
            }
            else
            {
                // When gates move, preserve bend points and only update segment endpoints
                UpdateSegmentEndpoints();
            }
        }
    }
    
    private void UpdateSegmentEndpoints()
    {
        // Update only the segment endpoints while preserving bend point positions
        if (_segments.Count == 3 && _bendPoints.Count == 2)
        {
            var bendPoint1 = _bendPoints[0];
            var bendPoint2 = _bendPoints[1];
            
            // Update segments with current start/end positions but keep bend points unchanged
            _segments[0].UpdatePoints(StartX, StartY, bendPoint1.X, bendPoint1.Y);
            _segments[1].UpdatePoints(bendPoint1.X, bendPoint1.Y, bendPoint2.X, bendPoint2.Y);
            _segments[2].UpdatePoints(bendPoint2.X, bendPoint2.Y, EndX, EndY);
            
            System.Diagnostics.Debug.WriteLine($"Updated wire segment endpoints, preserved bend points at ({bendPoint1.X:F0},{bendPoint1.Y:F0}) and ({bendPoint2.X:F0},{bendPoint2.Y:F0})");
        }
    }
    
    private void CalculateRouting()
    {
        var deltaX = EndX - StartX;
        var deltaY = EndY - StartY;
        
        // Clear existing segments and bend points
        _segments.Clear();
        _bendPoints.Clear();
        
        // Choose routing pattern based on which delta is larger
        if (Math.Abs(deltaX) > Math.Abs(deltaY))
        {
            // H-V-H pattern (horizontal-vertical-horizontal)
            _routingPattern = WireRoutingPattern.HVH;
            CalculateHVHRouting();
        }
        else
        {
            // V-H-V pattern (vertical-horizontal-vertical)
            _routingPattern = WireRoutingPattern.VHV;
            CalculateVHVRouting();
        }
    }
    
    private void CalculateHVHRouting()
    {
        // H-V-H: horizontal to middle, vertical to align, horizontal to end
        // Snap the middle X position to grid for clean alignment
        var midX = GridHelper.SnapToGrid((StartX + EndX) / 2);
        
        // Create bend points at grid-aligned positions
        var bendPoint1 = new BendPoint(midX, StartY);
        var bendPoint2 = new BendPoint(midX, EndY);
        
        // Subscribe to bend point changes
        bendPoint1.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(BendPoint.X) || e.PropertyName == nameof(BendPoint.Y))
                RecalculateSegments();
        };
        bendPoint2.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(BendPoint.X) || e.PropertyName == nameof(BendPoint.Y))
                RecalculateSegments();
        };
        
        _bendPoints.Add(bendPoint1);
        _bendPoints.Add(bendPoint2);
        
        // Create segments
        _segments.Add(new WireSegment(StartX, StartY, midX, StartY)); // Horizontal
        _segments.Add(new WireSegment(midX, StartY, midX, EndY));     // Vertical
        _segments.Add(new WireSegment(midX, EndY, EndX, EndY));       // Horizontal
        
        System.Diagnostics.Debug.WriteLine($"H-V-H routing: Start({StartX:F0},{StartY:F0}) -> Mid({midX:F0}) -> End({EndX:F0},{EndY:F0})");
    }
    
    private void CalculateVHVRouting()
    {
        // V-H-V: vertical to middle, horizontal to align, vertical to end
        // Snap the middle Y position to grid for clean alignment
        var midY = GridHelper.SnapToGrid((StartY + EndY) / 2);
        
        // Create bend points at grid-aligned positions
        var bendPoint1 = new BendPoint(StartX, midY);
        var bendPoint2 = new BendPoint(EndX, midY);
        
        // Subscribe to bend point changes
        bendPoint1.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(BendPoint.X) || e.PropertyName == nameof(BendPoint.Y))
                RecalculateSegments();
        };
        bendPoint2.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(BendPoint.X) || e.PropertyName == nameof(BendPoint.Y))
                RecalculateSegments();
        };
        
        _bendPoints.Add(bendPoint1);
        _bendPoints.Add(bendPoint2);
        
        // Create segments
        _segments.Add(new WireSegment(StartX, StartY, StartX, midY)); // Vertical
        _segments.Add(new WireSegment(StartX, midY, EndX, midY));     // Horizontal
        _segments.Add(new WireSegment(EndX, midY, EndX, EndY));       // Vertical
        
        System.Diagnostics.Debug.WriteLine($"V-H-V routing: Start({StartX:F0},{StartY:F0}) -> Mid({midY:F0}) -> End({EndX:F0},{EndY:F0})");
    }
    
    private void RecalculateSegments()
    {
        if (_bendPoints.Count == 2 && _segments.Count == 3)
        {
            var bendPoint1 = _bendPoints[0];
            var bendPoint2 = _bendPoints[1];
            
            // Update segments based on bend point positions
            // Note: Constraints are now applied in real-time during drag operations
            _segments[0].UpdatePoints(StartX, StartY, bendPoint1.X, bendPoint1.Y);
            _segments[1].UpdatePoints(bendPoint1.X, bendPoint1.Y, bendPoint2.X, bendPoint2.Y);
            _segments[2].UpdatePoints(bendPoint2.X, bendPoint2.Y, EndX, EndY);
        }
    }
    
}