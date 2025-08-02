namespace LogicSim.Core.Models.Gates;

public class LogicGate : BaseModel
{
    public GateType Type { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public LogicGate(GateType type, double x = 0, double y = 0)
    {
        Type = type;
        X = x;
        Y = y;
        Name = $"{type} Gate";
    }
}