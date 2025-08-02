using ReactiveUI;
using System.Reactive;

namespace LogicSim.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _greeting = "Welcome to LogicSim!";
    
    public string Greeting
    {
        get => _greeting;
        set => this.RaiseAndSetIfChanged(ref _greeting, value);
    }
    
    public ReactiveCommand<Unit, Unit> TestCommand { get; }
    
    public MainWindowViewModel()
    {
        TestCommand = ReactiveCommand.Create(() =>
        {
            Greeting = "Button clicked!";
        });
    }
}