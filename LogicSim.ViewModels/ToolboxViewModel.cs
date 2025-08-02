using ReactiveUI;
using System.Reactive;
using LogicSim.Core.Models.Gates;

namespace LogicSim.ViewModels;

public class ToolboxViewModel : ViewModelBase
{
    private readonly CircuitCanvasViewModel _canvasViewModel;
    
    public ToolboxViewModel(CircuitCanvasViewModel canvasViewModel)
    {
        _canvasViewModel = canvasViewModel;
        
        AddAndGateCommand = ReactiveCommand.Create(() => AddGate(GateType.And));
        AddOrGateCommand = ReactiveCommand.Create(() => AddGate(GateType.Or));
        AddNotGateCommand = ReactiveCommand.Create(() => AddGate(GateType.Not));
        AddXorGateCommand = ReactiveCommand.Create(() => AddGate(GateType.Xor));
        AddNandGateCommand = ReactiveCommand.Create(() => AddGate(GateType.Nand));
        AddNorGateCommand = ReactiveCommand.Create(() => AddGate(GateType.Nor));
    }
    
    public ReactiveCommand<Unit, Unit> AddAndGateCommand { get; }
    public ReactiveCommand<Unit, Unit> AddOrGateCommand { get; }
    public ReactiveCommand<Unit, Unit> AddNotGateCommand { get; }
    public ReactiveCommand<Unit, Unit> AddXorGateCommand { get; }
    public ReactiveCommand<Unit, Unit> AddNandGateCommand { get; }
    public ReactiveCommand<Unit, Unit> AddNorGateCommand { get; }
    
    private void AddGate(GateType gateType)
    {
        var position = _canvasViewModel.GetNextGatePosition();
        _canvasViewModel.AddGate(gateType, position.X, position.Y);
    }
}