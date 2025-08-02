using ReactiveUI;

namespace LogicSim.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private CircuitCanvasViewModel _canvasViewModel;
    private ToolboxViewModel _toolboxViewModel;
    
    public MainWindowViewModel()
    {
        _canvasViewModel = new CircuitCanvasViewModel();
        _toolboxViewModel = new ToolboxViewModel(_canvasViewModel);
    }
    
    public CircuitCanvasViewModel CanvasViewModel
    {
        get => _canvasViewModel;
        set => this.RaiseAndSetIfChanged(ref _canvasViewModel, value);
    }
    
    public ToolboxViewModel ToolboxViewModel
    {
        get => _toolboxViewModel;
        set => this.RaiseAndSetIfChanged(ref _toolboxViewModel, value);
    }
}