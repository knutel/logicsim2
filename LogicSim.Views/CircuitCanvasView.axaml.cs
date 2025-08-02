using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.LogicalTree;
using Avalonia.Collections;
using LogicSim.ViewModels;
using LogicSim.Views.Controls;
using LogicSim.Core.Utilities;
using System.Collections.Specialized;

namespace LogicSim.Views;

public partial class CircuitCanvasView : UserControl
{
    private GateViewModel? _draggedGate;
    private CircuitCanvasViewModel? _viewModel;
    private List<Line> _previewLines = new();
    private BendPoint? _draggedBendPoint;
    private double _bendPointDragOffsetX;
    private double _bendPointDragOffsetY;
    
    public CircuitCanvasView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        
        // Add canvas mouse events for wire preview
        CircuitCanvas.PointerMoved += OnCanvasPointerMoved;
        CircuitCanvas.PointerPressed += OnCanvasPointerPressed;
    }
    
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            if (_viewModel.Gates != null)
                _viewModel.Gates.CollectionChanged -= OnGatesCollectionChanged;
            if (_viewModel.Wires != null)
                _viewModel.Wires.CollectionChanged -= OnWiresCollectionChanged;
        }
        
        _viewModel = DataContext as CircuitCanvasViewModel;
        
        if (_viewModel != null)
        {
            if (_viewModel.Gates != null)
            {
                _viewModel.Gates.CollectionChanged += OnGatesCollectionChanged;
                
                // Add existing gates
                foreach (var gate in _viewModel.Gates)
                {
                    AddGateToCanvas(gate);
                }
            }
            
            if (_viewModel.Wires != null)
            {
                _viewModel.Wires.CollectionChanged += OnWiresCollectionChanged;
                
                // Add existing wires
                foreach (var wire in _viewModel.Wires)
                {
                    AddWireToCanvas(wire);
                }
            }
        }
    }
    
    private void OnGatesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (GateViewModel gate in e.NewItems)
            {
                AddGateToCanvas(gate);
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
        {
            foreach (GateViewModel gate in e.OldItems)
            {
                RemoveGateFromCanvas(gate);
            }
        }
    }
    
    private void OnWiresCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
        {
            foreach (WireViewModel wire in e.NewItems)
            {
                AddWireToCanvas(wire);
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
        {
            foreach (WireViewModel wire in e.OldItems)
            {
                RemoveWireFromCanvas(wire);
            }
        }
    }
    
    private void AddGateToCanvas(GateViewModel gateViewModel)
    {
        var gateView = new GateView { DataContext = gateViewModel };
        
        // Set initial position
        Canvas.SetLeft(gateView, gateViewModel.X);
        Canvas.SetTop(gateView, gateViewModel.Y);
        
        // Subscribe to position changes
        gateViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(GateViewModel.X))
            {
                Canvas.SetLeft(gateView, gateViewModel.X);
            }
            else if (e.PropertyName == nameof(GateViewModel.Y))
            {
                Canvas.SetTop(gateView, gateViewModel.Y);
            }
        };
        
        // Wire up drag events
        gateView.PointerPressed += OnGatePointerPressed;
        gateView.PointerMoved += OnGatePointerMoved;
        gateView.PointerReleased += OnGatePointerReleased;
        
        // Subscribe to pin click events (delayed to allow pin views to be created)
        gateView.Loaded += (s, e) => SubscribeToPinEvents(gateView);
        
        CircuitCanvas.Children.Add(gateView);
    }
    
    private void SubscribeToPinEvents(GateView gateView)
    {
        // Find all PinView controls in the GateView and subscribe to their click events
        var pinViews = GetDescendantsOfType<PinView>(gateView);
        foreach (var pinView in pinViews)
        {
            pinView.PinClicked += OnPinClicked;
        }
    }
    
    private static IEnumerable<T> GetDescendantsOfType<T>(Control parent) where T : Control
    {
        foreach (Control child in parent.GetLogicalChildren().OfType<Control>())
        {
            if (child is T match)
                yield return match;
            
            foreach (var descendant in GetDescendantsOfType<T>(child))
                yield return descendant;
        }
    }
    
    private void RemoveGateFromCanvas(GateViewModel gateViewModel)
    {
        var gateView = CircuitCanvas.Children
            .OfType<GateView>()
            .FirstOrDefault(g => g.DataContext == gateViewModel);
        
        if (gateView != null)
        {
            CircuitCanvas.Children.Remove(gateView);
        }
    }
    
    private void AddWireToCanvas(WireViewModel wireViewModel)
    {
        // Subscribe to segments collection changes
        wireViewModel.Segments.CollectionChanged += (s, e) => UpdateWireSegments(wireViewModel);
        wireViewModel.BendPoints.CollectionChanged += (s, e) => UpdateWireBendPoints(wireViewModel);
        
        // Add initial segments and bend points
        UpdateWireSegments(wireViewModel);
        UpdateWireBendPoints(wireViewModel);
        
        System.Diagnostics.Debug.WriteLine($"Wire added to canvas: {wireViewModel.StartPin?.Name} -> {wireViewModel.EndPin?.Name} with {wireViewModel.Segments.Count} segments");
    }
    
    private void UpdateWireSegments(WireViewModel wireViewModel)
    {
        // Remove existing segment lines for this wire
        var existingSegments = CircuitCanvas.Children
            .OfType<Line>()
            .Where(l => l.Tag is WireSegmentTag tag && tag.WireViewModel == wireViewModel)
            .ToList();
            
        foreach (var segment in existingSegments)
        {
            CircuitCanvas.Children.Remove(segment);
        }
        
        // Add new segment lines
        foreach (var segment in wireViewModel.Segments)
        {
            var line = new Line
            {
                StartPoint = new Avalonia.Point(segment.StartX, segment.StartY),
                EndPoint = new Avalonia.Point(segment.EndX, segment.EndY),
                Stroke = new SolidColorBrush(Color.Parse(wireViewModel.DisplayColor)),
                StrokeThickness = 2,
                Tag = new WireSegmentTag { WireViewModel = wireViewModel, Segment = segment }
            };
            
            // Subscribe to segment position changes
            segment.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(WireSegment.StartX) || e.PropertyName == nameof(WireSegment.StartY))
                {
                    line.StartPoint = new Avalonia.Point(segment.StartX, segment.StartY);
                }
                else if (e.PropertyName == nameof(WireSegment.EndX) || e.PropertyName == nameof(WireSegment.EndY))
                {
                    line.EndPoint = new Avalonia.Point(segment.EndX, segment.EndY);
                }
            };
            
            // Add segment line at the beginning so it renders behind gates
            CircuitCanvas.Children.Insert(0, line);
        }
    }
    
    private void UpdateWireBendPoints(WireViewModel wireViewModel)
    {
        // Remove existing bend point circles for this wire
        var existingBendPoints = CircuitCanvas.Children
            .OfType<Ellipse>()
            .Where(e => e.Tag is BendPointTag tag && tag.WireViewModel == wireViewModel)
            .ToList();
            
        foreach (var bendPoint in existingBendPoints)
        {
            CircuitCanvas.Children.Remove(bendPoint);
        }
        
        // Add new bend point circles
        foreach (var bendPoint in wireViewModel.BendPoints)
        {
            var circle = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.Parse(bendPoint.DisplayColor)),
                Stroke = new SolidColorBrush(Color.Parse("#000000")),
                StrokeThickness = 1,
                Cursor = new Cursor(StandardCursorType.Hand),
                Tag = new BendPointTag { WireViewModel = wireViewModel, BendPoint = bendPoint }
            };
            
            // Add bend point event handlers
            circle.PointerPressed += OnBendPointPointerPressed;
            circle.PointerMoved += OnBendPointPointerMoved;
            circle.PointerReleased += OnBendPointPointerReleased;
            circle.PointerEntered += OnBendPointPointerEntered;
            circle.PointerExited += OnBendPointPointerExited;
            
            // Position the circle
            Canvas.SetLeft(circle, bendPoint.X - 4); // Center the 8px circle
            Canvas.SetTop(circle, bendPoint.Y - 4);
            
            // Subscribe to bend point position changes
            bendPoint.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(BendPoint.X))
                {
                    Canvas.SetLeft(circle, bendPoint.X - 4);
                }
                else if (e.PropertyName == nameof(BendPoint.Y))
                {
                    Canvas.SetTop(circle, bendPoint.Y - 4);
                }
                else if (e.PropertyName == nameof(BendPoint.DisplayColor))
                {
                    circle.Fill = new SolidColorBrush(Color.Parse(bendPoint.DisplayColor));
                }
            };
            
            // Add bend point circle above wires but below gates
            CircuitCanvas.Children.Add(circle);
        }
    }
    
    private void RemoveWireFromCanvas(WireViewModel wireViewModel)
    {
        // Remove all segment lines for this wire
        var segmentLines = CircuitCanvas.Children
            .OfType<Line>()
            .Where(l => l.Tag is WireSegmentTag tag && tag.WireViewModel == wireViewModel)
            .ToList();
            
        foreach (var line in segmentLines)
        {
            CircuitCanvas.Children.Remove(line);
        }
        
        // Remove all bend point circles for this wire
        var bendPointCircles = CircuitCanvas.Children
            .OfType<Ellipse>()
            .Where(e => e.Tag is BendPointTag tag && tag.WireViewModel == wireViewModel)
            .ToList();
            
        foreach (var circle in bendPointCircles)
        {
            CircuitCanvas.Children.Remove(circle);
        }
        
        System.Diagnostics.Debug.WriteLine($"Wire removed from canvas: {wireViewModel.StartPin?.Name} -> {wireViewModel.EndPin?.Name}");
    }
    
    private void OnGatePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control control && control.DataContext is GateViewModel gateViewModel)
        {
            _draggedGate = gateViewModel;
            var point = e.GetPosition(CircuitCanvas);
            gateViewModel.StartDrag(point.X, point.Y);
            e.Pointer.Capture(control);
            e.Handled = true;
        }
    }
    
    private void OnGatePointerMoved(object? sender, PointerEventArgs e)
    {
        if (_draggedGate != null && _draggedGate.IsDragging)
        {
            var point = e.GetPosition(CircuitCanvas);
            _draggedGate.UpdatePosition(point.X, point.Y);
            e.Handled = true;
        }
    }
    
    private void OnGatePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_draggedGate != null)
        {
            _draggedGate.EndDrag();
            _draggedGate = null;
            if (sender is Control control)
            {
                e.Pointer.Capture(null);
            }
            e.Handled = true;
        }
    }
    
    private void OnPinClicked(object? sender, PinClickedEventArgs e)
    {
        if (_viewModel == null) return;
        
        var pinViewModel = e.PinViewModel;
        
        if (_viewModel.WiringState == WiringState.Idle)
        {
            // Start a new wire
            _viewModel.StartWire(pinViewModel);
            pinViewModel.IsWireSource = true;
        }
        else if (_viewModel.WiringState != WiringState.Idle && _viewModel.WireSourcePin != null)
        {
            // Save reference to source pin before completing wire (CompleteWire calls CancelWire which nulls WireSourcePin)
            var sourcePin = _viewModel.WireSourcePin;
            
            // Complete the wire
            _viewModel.CompleteWire(pinViewModel);
            sourcePin.IsWireSource = false;
            HidePreviewLine();
        }
    }
    
    private void OnCanvasPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_viewModel == null) return;
        
        if (_viewModel.IsWiring)
        {
            var point = e.GetPosition(CircuitCanvas);
            _viewModel.UpdatePreviewLine(point.X, point.Y);
            UpdatePreviewLine();
        }
    }
    
    private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_viewModel == null) return;
        
        // If we're in wiring mode and clicked on empty canvas, cancel the wire
        if (_viewModel.IsWiring)
        {
            // Save reference to source pin before canceling wire (CancelWire nulls WireSourcePin)
            var sourcePin = _viewModel.WireSourcePin;
            _viewModel.CancelWire();
            if (sourcePin != null)
            {
                sourcePin.IsWireSource = false;
            }
            HidePreviewLine();
            e.Handled = true;
        }
    }
    
    private void UpdatePreviewLine()
    {
        if (_viewModel == null || !_viewModel.IsWiring || _viewModel.WireSourcePin == null) return;
        
        // Remove existing preview lines
        foreach (var line in _previewLines)
        {
            CircuitCanvas.Children.Remove(line);
        }
        _previewLines.Clear();
        
        // Calculate start position from source pin
        var sourcePin = _viewModel.WireSourcePin;
        var sourceGate = _viewModel.FindGateContainingPin(sourcePin);
        
        if (sourceGate != null)
        {
            var startX = sourceGate.X + sourcePin.RelativeX + 6; // Center of pin
            var startY = sourceGate.Y + sourcePin.RelativeY + 6;
            var endX = _viewModel.PreviewEndX;
            var endY = _viewModel.PreviewEndY;
            
            // Calculate 3-segment preview routing
            CalculatePreviewRouting(startX, startY, endX, endY);
        }
    }
    
    private void CalculatePreviewRouting(double startX, double startY, double endX, double endY)
    {
        var deltaX = endX - startX;
        var deltaY = endY - startY;
        
        List<(double x1, double y1, double x2, double y2)> segments;
        
        // Choose routing pattern based on which delta is larger
        if (Math.Abs(deltaX) > Math.Abs(deltaY))
        {
            // H-V-H pattern
            var midX = (startX + endX) / 2;
            segments = new List<(double, double, double, double)>
            {
                (startX, startY, midX, startY), // Horizontal
                (midX, startY, midX, endY),     // Vertical  
                (midX, endY, endX, endY)        // Horizontal
            };
        }
        else
        {
            // V-H-V pattern
            var midY = (startY + endY) / 2;
            segments = new List<(double, double, double, double)>
            {
                (startX, startY, startX, midY), // Vertical
                (startX, midY, endX, midY),     // Horizontal
                (endX, midY, endX, endY)        // Vertical
            };
        }
        
        // Create preview line segments
        foreach (var (x1, y1, x2, y2) in segments)
        {
            var line = new Line
            {
                StartPoint = new Avalonia.Point(x1, y1),
                EndPoint = new Avalonia.Point(x2, y2),
                Stroke = new SolidColorBrush(Color.Parse("#FFD700")), // Gold color
                StrokeThickness = 2,
                StrokeDashArray = new AvaloniaList<double> { 5, 5 }, // Dotted line
                IsHitTestVisible = false // Don't interfere with mouse events
            };
            
            _previewLines.Add(line);
            CircuitCanvas.Children.Add(line);
        }
    }
    
    private void HidePreviewLine()
    {
        foreach (var line in _previewLines)
        {
            CircuitCanvas.Children.Remove(line);
        }
        _previewLines.Clear();
    }
    
    private void OnBendPointPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Ellipse circle && circle.Tag is BendPointTag tag)
        {
            _draggedBendPoint = tag.BendPoint;
            if (_draggedBendPoint != null)
            {
                var point = e.GetPosition(CircuitCanvas);
                _bendPointDragOffsetX = point.X - _draggedBendPoint.X;
                _bendPointDragOffsetY = point.Y - _draggedBendPoint.Y;
                _draggedBendPoint.IsDragging = true;
                e.Pointer.Capture(circle);
                e.Handled = true;
                
                System.Diagnostics.Debug.WriteLine($"Started dragging bend point at ({_draggedBendPoint.X:F0}, {_draggedBendPoint.Y:F0})");
            }
        }
    }
    
    private void OnBendPointPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_draggedBendPoint != null && _draggedBendPoint.IsDragging)
        {
            var point = e.GetPosition(CircuitCanvas);
            var newX = point.X - _bendPointDragOffsetX;
            var newY = point.Y - _bendPointDragOffsetY;
            
            // Snap bend point position to grid during drag
            var snappedPosition = GridHelper.SnapPoint(newX, newY);
            _draggedBendPoint.MoveTo(snappedPosition.X, snappedPosition.Y);
            
            e.Handled = true;
        }
    }
    
    private void OnBendPointPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_draggedBendPoint != null)
        {
            _draggedBendPoint.IsDragging = false;
            System.Diagnostics.Debug.WriteLine($"Finished dragging bend point to ({_draggedBendPoint.X:F0}, {_draggedBendPoint.Y:F0})");
            _draggedBendPoint = null;
            
            if (sender is Control control)
            {
                e.Pointer.Capture(null);
            }
            e.Handled = true;
        }
    }
    
    private void OnBendPointPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Ellipse circle && circle.Tag is BendPointTag tag && tag.BendPoint != null)
        {
            tag.BendPoint.IsHovered = true;
        }
    }
    
    private void OnBendPointPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Ellipse circle && circle.Tag is BendPointTag tag && tag.BendPoint != null)
        {
            tag.BendPoint.IsHovered = false;
        }
    }
    
}

// Tag classes for identifying canvas elements
public class WireSegmentTag
{
    public WireViewModel? WireViewModel { get; set; }
    public WireSegment? Segment { get; set; }
}

public class BendPointTag
{
    public WireViewModel? WireViewModel { get; set; }
    public BendPoint? BendPoint { get; set; }
}