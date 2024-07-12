namespace Necromation.components;

public class FuelComponent
{
    public Inventory InputInventory { private get; init; }
    public float FuelTime { get; private set; }

    public void _Process(double delta)
    {
        if (FuelTime > 0) FuelTime -= (float)delta;
    }

    public bool DrawPower()
    {
        if (FuelTime <= 0 && InputInventory.Remove("Coal Ore"))
        {
            FuelTime = 10;
        }
        return FuelTime > 0;
    }
}