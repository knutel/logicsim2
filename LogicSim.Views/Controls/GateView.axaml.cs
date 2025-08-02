using Avalonia.Controls;
using Avalonia.LogicalTree;
using LogicSim.ViewModels;
using System.Collections.Specialized;
using System.Linq;

namespace LogicSim.Views.Controls;

public partial class GateView : UserControl
{
    private Canvas? _canvas;
    
    public GateView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }
    
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is GateViewModel gateViewModel)
        {
            // Find the main Canvas
            _canvas = this.GetLogicalDescendants().OfType<Canvas>().FirstOrDefault();
            
            if (_canvas != null)
            {
                // Subscribe to pin collection changes
                gateViewModel.InputPins.CollectionChanged += OnPinsChanged;
                gateViewModel.OutputPins.CollectionChanged += OnPinsChanged;
                
                // Add initial pins
                UpdatePinViews(gateViewModel);
            }
        }
    }
    
    private void OnPinsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (DataContext is GateViewModel gateViewModel && _canvas != null)
        {
            UpdatePinViews(gateViewModel);
        }
    }
    
    private void UpdatePinViews(GateViewModel gateViewModel)
    {
        if (_canvas == null) return;
        
        // Remove existing pin views (but keep other elements)
        var pinViews = _canvas.Children.OfType<PinView>().ToList();
        foreach (var pinView in pinViews)
        {
            _canvas.Children.Remove(pinView);
        }
        
        // Add input pins
        foreach (var pinViewModel in gateViewModel.InputPins)
        {
            var pinView = new PinView { DataContext = pinViewModel };
            Canvas.SetLeft(pinView, pinViewModel.RelativeX);
            Canvas.SetTop(pinView, pinViewModel.RelativeY);
            _canvas.Children.Add(pinView);
            
            // Debug: Add position info
            System.Diagnostics.Debug.WriteLine($"Input pin at RelativeX={pinViewModel.RelativeX}, RelativeY={pinViewModel.RelativeY}");
        }
        
        // Add output pins
        foreach (var pinViewModel in gateViewModel.OutputPins)
        {
            var pinView = new PinView { DataContext = pinViewModel };
            Canvas.SetLeft(pinView, pinViewModel.RelativeX);
            Canvas.SetTop(pinView, pinViewModel.RelativeY);
            _canvas.Children.Add(pinView);
            
            // Debug: Add position info
            System.Diagnostics.Debug.WriteLine($"Output pin at RelativeX={pinViewModel.RelativeX}, RelativeY={pinViewModel.RelativeY}");
        }
    }
}