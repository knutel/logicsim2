# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Development Commands

### Building and Running
```bash
# Build entire solution
dotnet build

# Run the application
dotnet run --project LogicSim.Desktop

# Build specific project
dotnet build LogicSim.Core

# Run tests (when added)
dotnet test
```

### Project Structure Commands
```bash
# Add new project to solution
dotnet sln add ProjectName/ProjectName.csproj

# Add project reference
dotnet add ProjectA reference ProjectB/ProjectB.csproj

# Add NuGet package
dotnet add package PackageName --version x.x.x
```

## Architecture Overview

This is a .NET 8 Avalonia UI application implementing a digital logic circuit simulator using strict MVVM architecture with ReactiveUI.

### Layer Dependencies (strict hierarchy)
```
LogicSim.Desktop (entry point)
├── LogicSim.Views (UI layer)
├── LogicSim.ViewModels (presentation logic)
└── LogicSim.Core (domain models, no dependencies)
```

**Critical**: Core layer must never reference UI layers. ViewModels can reference Core but not Views. Views can reference both ViewModels and Core.

### Key Architecture Patterns

**ReactiveUI MVVM**: All ViewModels inherit from `ViewModelBase` (which extends `ReactiveObject`). Use `this.RaiseAndSetIfChanged(ref field, value)` for observable properties and `ReactiveCommand<TInput, TOutput>` for commands.

**ViewLocator Pattern**: Automatic View/ViewModel mapping via `ViewLocator.cs`. ViewModels ending in "ViewModel" automatically map to Views ending in "View" in the Views namespace.

**Gate System**: 
- `LogicGate` (Core) contains position and type data
- `GateViewModel` wraps LogicGate with reactive properties and drag behavior  
- `GateView` provides visual representation
- `CircuitCanvasViewModel` manages collection of gates with smart positioning

**Drag and Drop**: Implemented via pointer events in Views with state managed in ViewModels. Pattern: `StartDrag()` → `UpdatePosition()` → `EndDrag()` with offset calculations.

## Current Features

- **Toolbox**: Left sidebar with buttons to create AND, OR, NOT, XOR, NAND, NOR gates
- **Canvas**: Scrollable 2000x2000 canvas with drag-and-drop gates
- **Smart Positioning**: New gates auto-positioned in 6-column grid layout
- **Reactive UI**: All gate positions and state updates use ReactiveUI bindings

## Key Implementation Details

### Adding New Gate Types
1. Add to `GateType` enum in Core
2. Add button and command in `ToolboxViewModel`
3. Add button UI in `ToolboxView.axaml`
4. Visual representation handled automatically by `GateView`

### XAML Considerations
- Use proper XML entity encoding (`&amp;` not `&`)
- Canvas positioning via `Canvas.Left` and `Canvas.Top` attached properties
- Dark theme colors: `#2D3142` (primary), `#4F5D75` (secondary), `#252525` (canvas)

### Manual Canvas Management
The `CircuitCanvasView` manually manages Canvas children rather than using ItemsControl due to Avalonia limitations. Gates are added/removed via `AddGateToCanvas()` and `RemoveGateFromCanvas()` methods with proper event wiring.

### ViewModels Coordinate
- `MainWindowViewModel` owns both `CanvasViewModel` and `ToolboxViewModel`
- `ToolboxViewModel` receives `CanvasViewModel` reference to call `AddGate()`
- `CircuitCanvasViewModel.GetNextGatePosition()` provides automatic grid positioning

## Testing Strategy
- Domain logic: Pure unit tests (no UI)
- ViewModels: Test property changes, command execution
- No UI testing initially

## Avoid
- Direct UI manipulation from domain
- WPF-specific patterns
- Blocking operations on UI thread
- Manual property change notifications (use ReactiveUI)
- Business logic in ViewModels

## Development Philosophy

This project follows incremental development with small, working features. Focus on:
- Clean MVVM separation
- Reactive programming patterns
- Working features over complex architecture
- Small commits with specific functionality

Avoid "big bang" implementations - build features piece by piece with immediate visual feedback.