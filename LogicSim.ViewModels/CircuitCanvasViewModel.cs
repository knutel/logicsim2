using System.Collections.ObjectModel;
using ReactiveUI;
using LogicSim.Core.Models.Gates;

namespace LogicSim.ViewModels;

public class CircuitCanvasViewModel : ViewModelBase
{
    private ObservableCollection<GateViewModel> _gates;
    private int _gateCounter = 0;
    
    public CircuitCanvasViewModel()
    {
        _gates = new ObservableCollection<GateViewModel>();
    }
    
    public ObservableCollection<GateViewModel> Gates
    {
        get => _gates;
        set => this.RaiseAndSetIfChanged(ref _gates, value);
    }
    
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
}