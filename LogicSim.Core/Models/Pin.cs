namespace LogicSim.Core.Models;

public class Pin : BaseModel
{
    public PinDirection Direction { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsConnected { get; set; } = false;
    public Guid GateId { get; set; }
    
    public Pin(PinDirection direction, string name, Guid gateId)
    {
        Direction = direction;
        Name = name;
        GateId = gateId;
    }
}