using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.LogicalTree;
using Avalonia.Collections;
using LogicSim.ViewModels;
using LogicSim.Views.Controls;
using System.Collections.Specialized;

namespace LogicSim.Views;

public partial class CircuitCanvasView : UserControl
{
    private GateViewModel? _draggedGate;
    private CircuitCanvasViewModel? _viewModel;
    private Line? _previewLine;
    
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
        var line = new Line
        {
            StartPoint = new Avalonia.Point(wireViewModel.StartX, wireViewModel.StartY),
            EndPoint = new Avalonia.Point(wireViewModel.EndX, wireViewModel.EndY),
            Stroke = new SolidColorBrush(Color.Parse(wireViewModel.DisplayColor)),
            StrokeThickness = 2,
            Tag = wireViewModel // Store reference for removal
        };
        
        // Subscribe to wire position changes
        wireViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(WireViewModel.StartX) || e.PropertyName == nameof(WireViewModel.StartY))
            {
                line.StartPoint = new Avalonia.Point(wireViewModel.StartX, wireViewModel.StartY);
            }
            else if (e.PropertyName == nameof(WireViewModel.EndX) || e.PropertyName == nameof(WireViewModel.EndY))
            {
                line.EndPoint = new Avalonia.Point(wireViewModel.EndX, wireViewModel.EndY);
            }
        };
        
        // Add wire line at the beginning so it renders behind gates
        CircuitCanvas.Children.Insert(0, line);
        System.Diagnostics.Debug.WriteLine($"Wire added to canvas: {wireViewModel.StartPin?.Name} -> {wireViewModel.EndPin?.Name}");
    }
    
    private void RemoveWireFromCanvas(WireViewModel wireViewModel)
    {
        var line = CircuitCanvas.Children
            .OfType<Line>()
            .FirstOrDefault(l => l.Tag == wireViewModel);
        
        if (line != null)
        {
            CircuitCanvas.Children.Remove(line);
            System.Diagnostics.Debug.WriteLine($"Wire removed from canvas: {wireViewModel.StartPin?.Name} -> {wireViewModel.EndPin?.Name}");
        }
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
        
        if (_previewLine == null)
        {
            _previewLine = new Line
            {
                Stroke = new SolidColorBrush(Color.Parse("#FFD700")), // Gold color
                StrokeThickness = 2,
                StrokeDashArray = new AvaloniaList<double> { 5, 5 }, // Dotted line
                IsHitTestVisible = false // Don't interfere with mouse events
            };
            CircuitCanvas.Children.Add(_previewLine);
        }
        
        // Calculate start position from source pin - need to find the gate containing this pin
        var sourcePin = _viewModel.WireSourcePin;
        var sourceGate = _viewModel.FindGateContainingPin(sourcePin);
        
        if (sourceGate != null)
        {
            var startX = sourceGate.X + sourcePin.RelativeX + 6; // Center of pin
            var startY = sourceGate.Y + sourcePin.RelativeY + 6;
            
            _previewLine.StartPoint = new Avalonia.Point(startX, startY);
            _previewLine.EndPoint = new Avalonia.Point(_viewModel.PreviewEndX, _viewModel.PreviewEndY);
        }
    }
    
    private void HidePreviewLine()
    {
        if (_previewLine != null)
        {
            CircuitCanvas.Children.Remove(_previewLine);
            _previewLine = null;
        }
    }
}