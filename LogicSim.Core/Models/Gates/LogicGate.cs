using LogicSim.Core.Models;

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
        const double gateWidth = 80;
        const double gateHeight = 50;
        const double pinRadius = 6; // Half of pin width (12px)
        
        // Position input pins on left edge (centered on border)
        for (int i = 0; i < InputPins.Count; i++)
        {
            var pin = InputPins[i];
            pin.X = -pinRadius; // Center pin on gate left edge
            pin.Y = gateHeight * (i + 1) / (InputPins.Count + 1) - pinRadius; // Center pin on calculated Y
        }
        
        // Position output pins on right edge (centered on border)
        for (int i = 0; i < OutputPins.Count; i++)
        {
            var pin = OutputPins[i];
            pin.X = gateWidth - pinRadius; // Center pin on gate right edge
            pin.Y = gateHeight * (i + 1) / (OutputPins.Count + 1) - pinRadius; // Center pin on calculated Y
        }
    }
}