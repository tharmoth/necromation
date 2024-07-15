using System;
using Godot;
using Necromation.components;
using Necromation.interfaces;

public class Manaforge : Building, IInteractable, IPowerSource, FurnaceAnimationComponent.IAnimatable
{
    /**************************************************************************
     * Events                                                                 *
     **************************************************************************/
    public event Action StartAnimation;
    public event Action StopAnimation;
    
    /**************************************************************************
     * Public Variables                                                       *
     **************************************************************************/
    #region Building Implementation
    public override Vector2I BuildingSize => Vector2I.One * 3;
    public override string ItemType => "Manaforge";
    #endregion

    #region IPowerSource Implementation
    public float EnergyStored { get; set; }
    private float PowerMax => 1000.0f;
    public float PowerLimit => 100.0f;
    public bool Disconnected
    {
        set
        {
            GD.Print($"Manaforge Disconnected: {value}");
        }
    }
    #endregion
    
    /**************************************************************************
     * Private Variables                                                      *
     **************************************************************************/
    private readonly Inventory _inventory = new();
    private readonly FuelComponent _fuelComponent;
    
    public Manaforge() : base()
    {
        _fuelComponent = new FuelComponent() {InputInventory = _inventory};
        new FurnaceAnimationComponent(this);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _fuelComponent._Process(delta);
        if (_fuelComponent.CanDrawPower() && EnergyStored <= PowerMax)
        {
            _fuelComponent.DrawPower();
            EnergyStored += PowerLimit * (float) delta;
            StartAnimation?.Invoke();
        }
        else
        {
            StopAnimation?.Invoke();
        }
    }

    public void Interact(Inventory playerInventory)
    {
        ContainerGui.Display(playerInventory, _inventory , "Manaforge");
    }

}