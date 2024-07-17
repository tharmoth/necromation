using Godot;

namespace Necromation.components.animation;

public class PowerSourceComponent : IPowerSource
{
    /**************************************************************************
     * Public Variables                                                       *
     **************************************************************************/
    #region IPowerSource Implementation
    public float Energy { get; set; }
    public float EnergyMax => 1000.0f;
    public float Power => 100.0f;
    public bool Disconnected
    {
        set
        {
            GD.Print($"Manaforge Disconnected: {value}");
        }
    }
    #endregion
    
    public required FuelComponent FuelComponent { private get; init; }
    
    /**************************************************************************
     * Godot Functions                                                       *
     **************************************************************************/
    public void _Process(double delta)
    {
        if (!FuelComponent.CanDrawPower() || !CanAddPower(delta)) return;
        
        FuelComponent.DrawPower();
        Energy += Power * (float) delta;
    }

    /**************************************************************************
     * private Functions                                                       *
     **************************************************************************/
    private bool CanAddPower(double delta) => EnergyMax >= Energy + Power * (float) delta;
}