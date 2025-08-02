namespace LogicSim.Core.Utilities;

public static class GridHelper
{
    public const double GridSize = 10.0;
    
    public static double SnapToGrid(double value)
    {
        return Math.Round(value / GridSize) * GridSize;
    }
    
    public static (double X, double Y) SnapPoint(double x, double y)
    {
        return (SnapToGrid(x), SnapToGrid(y));
    }
    
    public static double SnapToGridFloor(double value)
    {
        return Math.Floor(value / GridSize) * GridSize;
    }
    
    public static double SnapToGridCeil(double value)
    {
        return Math.Ceiling(value / GridSize) * GridSize;
    }
}