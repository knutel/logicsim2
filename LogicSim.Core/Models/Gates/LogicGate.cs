using LogicSim.Core.Models;
using LogicSim.Core.Utilities;

namespace LogicSim.Core.Models.Gates;

public class LogicGate : BaseModel
{
    public GateType Type { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Pin> InputPins { get; set; } = new();
    public List<Pin> OutputPins { get; set; } = new();
    
    public LogicGate(GateType type, double x = 0, double y = 0)
    {
        Type = type;
        X = x;
        Y = y;
        Name = $"{type} Gate";
        InitializePins();
        UpdatePinPositions();
    }
    
    private void InitializePins()
    {
        InputPins.Clear();
        OutputPins.Clear();
        
        // Create pins based on gate type
        switch (Type)
        {
            case GateType.Not:
                InputPins.Add(new Pin(PinDirection.Input, "Input", Id));
                break;
            
            case GateType.And:
            case GateType.Or:
            case GateType.Xor:
            case GateType.Nand:
            case GateType.Nor:
                InputPins.Add(new Pin(PinDirection.Input, "Input A", Id));
                InputPins.Add(new Pin(PinDirection.Input, "Input B", Id));
                break;
        }
        
        // All gates have one output
        OutputPins.Add(new Pin(PinDirection.Output, "Output", Id));
    }
    
    public void UpdatePinPositions()
    {
        const double gateBodyOffsetX = 10; // Gate body starts at Canvas.Left="10"
        const double gateBodyOffsetY = 10; // Gate body starts at Canvas.Top="10"
        const double gateWidth = 80;
        const double gateHeight = 40;
        const double pinRadius = 6; // Half of pin width (12px)
        
        // Position input pins on left edge of gate body
        // Pin positions are relative to the gate canvas (not gate body)
        for (int i = 0; i < InputPins.Count; i++)
        {
            var pin = InputPins[i];
            // Position at left edge of gate body
            pin.X = gateBodyOffsetX - pinRadius; // 10 - 6 = 4 (pin center at x=10)
            
            // Grid-aligned Y positions based on pin count
            if (InputPins.Count == 1)
            {
                // Center single pin in gate body
                pin.Y = gateBodyOffsetY + (gateHeight / 2) - pinRadius; // 10 + 20 - 6 = 24
            }
            else if (InputPins.Count == 2)
            {
                // Two pins at 1/3 and 2/3 of gate height
                // For clean 10px grid alignment
                pin.Y = gateBodyOffsetY + ((i == 0) ? 10 : 30) - pinRadius; // Results in Y=14 or Y=34
            }
            else
            {
                // For more pins, distribute evenly
                double spacing = gateHeight / (InputPins.Count + 1);
                pin.Y = gateBodyOffsetY + GridHelper.SnapToGrid(spacing * (i + 1)) - pinRadius;
            }
        }
        
        // Position output pins on right edge of gate body
        for (int i = 0; i < OutputPins.Count; i++)
        {
            var pin = OutputPins[i];
            // Position at right edge of gate body
            pin.X = gateBodyOffsetX + gateWidth - pinRadius; // 10 + 80 - 6 = 84 (pin center at x=90)
            
            // Grid-aligned Y positions based on pin count
            if (OutputPins.Count == 1)
            {
                // Center single pin in gate body
                pin.Y = gateBodyOffsetY + (gateHeight / 2) - pinRadius; // 10 + 20 - 6 = 24
            }
            else
            {
                // For multiple pins, distribute evenly
                double spacing = gateHeight / (OutputPins.Count + 1);
                pin.Y = gateBodyOffsetY + GridHelper.SnapToGrid(spacing * (i + 1)) - pinRadius;
            }
        }
    }
}