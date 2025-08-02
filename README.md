# LogicSim - Digital Logic Simulator

> [!CAUTION]
> This is an experiment to see what Claude Code can do. This is my second attempt. The first one got way too vibe-y and starting to break under its own weight. My intention this time is to keep a bit shorter leash on CC by giving more details and not accepting crappy code. Commits labeled as co-authored by Claude are entirely Claude’s work, while those without that label are mine.
> Almost all demonstrations of AI coding, or vibe coding, are doing trivial ToDo apps or the like. This is an attempt at doing something useful instead.

A .NET 8 Avalonia application using MVVM pattern with ReactiveUI for building digital logic circuits.

## Solution Structure

```
LogicSim/
├── LogicSim.Core/           # Core business logic and domain models
│   ├── Models/              # Domain models
│   └── Services/            # Business services
├── LogicSim.ViewModels/     # ViewModels layer (MVVM)
│   ├── ViewModelBase.cs    # Base class for all ViewModels
│   └── MainWindowViewModel.cs
├── LogicSim.Views/          # Views layer (UI)
│   ├── MainWindow.axaml    # Main window XAML
│   └── MainWindow.axaml.cs # Main window code-behind
└── LogicSim.Desktop/        # Desktop application entry point
    ├── Program.cs           # Application entry point
    ├── App.axaml           # Application resources
    ├── App.axaml.cs        # Application class
    └── ViewLocator.cs      # View/ViewModel mapping
```

## Technologies

- **.NET 8**: Latest LTS version of .NET
- **Avalonia UI 11.2.2**: Cross-platform UI framework
- **ReactiveUI 20.1.63**: MVVM framework with reactive extensions
- **Fluent Theme**: Modern UI theme

## Architecture

The solution follows a layered architecture with MVVM pattern:

1. **Core Layer** (`LogicSim.Core`): Contains business logic, domain models, and services. No UI dependencies.

2. **ViewModels Layer** (`LogicSim.ViewModels`): Contains ViewModels that implement presentation logic using ReactiveUI's ReactiveObject base class.

3. **Views Layer** (`LogicSim.Views`): Contains Avalonia UI views (XAML) and their code-behind files.

4. **Desktop Layer** (`LogicSim.Desktop`): The executable project that bootstraps the application.

## Dependencies Flow

```
LogicSim.Desktop
    ├── LogicSim.Views
    │   ├── LogicSim.ViewModels
    │   │   └── LogicSim.Core
    │   └── LogicSim.Core
    ├── LogicSim.ViewModels
    │   └── LogicSim.Core
    └── LogicSim.Core
```

## Building and Running

### Prerequisites
- .NET 8 SDK

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run --project LogicSim.Desktop
```

## Development

The application uses:
- **ReactiveUI** for reactive programming and MVVM implementation
- **Observable properties** for data binding
- **Commands** for user interactions
- **ViewLocator** for automatic View/ViewModel resolution

## License

MIT License - See LICENSE file for details