namespace LogicSim.Core.Models;

public class Connection : BaseModel
{
    public Guid OutputPinId { get; set; }
    public Guid InputPinId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public Connection(Guid outputPinId, Guid inputPinId)
    {
        OutputPinId = outputPinId;
        InputPinId = inputPinId;
        Name = $"Connection {Id:N[..8]}";
    }
}