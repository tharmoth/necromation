namespace Necromation.components;

public class FuelComponent
{
    public const float CoalBurnTime = 10.0f;
    public Inventory InputInventory { private get; init; }
    public float FuelTime { get; private set; }
    public float Progress => FuelTime / CoalBurnTime;

    public void _Process(double delta)
    {
        if (FuelTime > 0) FuelTime -= (float)delta;
    }

    public int DrawPower()
    {
        if (FuelTime <= 0 && InputInventory.Remove("Coal Ore"))
        {
            FuelTime = CoalBurnTime;
        }
        return FuelTime > 0 ? 100 : 0;
    }
    
    public bool CanDrawPower()
    {
        return FuelTime > 0 || InputInventory.CountItem("Coal Ore") > 0;
    }
}