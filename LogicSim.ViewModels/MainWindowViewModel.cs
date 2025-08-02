using ReactiveUI;

namespace LogicSim.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private CircuitCanvasViewModel _canvasViewModel;
    
    public MainWindowViewModel()
    {
        _canvasViewModel = new CircuitCanvasViewModel();
    }
    
    public CircuitCanvasViewModel CanvasViewModel
    {
        get => _canvasViewModel;
        set => this.RaiseAndSetIfChanged(ref _canvasViewModel, value);
    }
}