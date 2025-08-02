using Avalonia.Controls;
using Avalonia.Input;
using LogicSim.ViewModels;
using LogicSim.Views.Controls;
using System.Collections.Specialized;

namespace LogicSim.Views;

public partial class CircuitCanvasView : UserControl
{
    private GateViewModel? _draggedGate;
    private CircuitCanvasViewModel? _viewModel;
    
    public CircuitCanvasView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }
    
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null && _viewModel.Gates != null)
        {
            _viewModel.Gates.CollectionChanged -= OnGatesCollectionChanged;
        }
        
        _viewModel = DataContext as CircuitCanvasViewModel;
        
        if (_viewModel != null && _viewModel.Gates != null)
        {
            _viewModel.Gates.CollectionChanged += OnGatesCollectionChanged;
            
            // Add existing gates
            foreach (var gate in _viewModel.Gates)
            {
                AddGateToCanvas(gate);
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
        
        CircuitCanvas.Children.Add(gateView);
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
}