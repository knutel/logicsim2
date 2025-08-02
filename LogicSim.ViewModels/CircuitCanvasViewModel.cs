using System.Collections.ObjectModel;
using ReactiveUI;
using LogicSim.Core.Models.Gates;

namespace LogicSim.ViewModels;

public class CircuitCanvasViewModel : ViewModelBase
{
    private ObservableCollection<GateViewModel> _gates;
    
    public CircuitCanvasViewModel()
    {
        _gates = new ObservableCollection<GateViewModel>();
        
        // Add a sample AND gate to start with
        var andGate = new LogicGate(GateType.And, 100, 100);
        _gates.Add(new GateViewModel(andGate));
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
}