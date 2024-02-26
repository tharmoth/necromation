using Godot;
using System;
using System.Collections.Generic;
using Necromation;

public partial class SquadUnit : TextureRect
{
	public string UnitName { get; }
    public bool Selected = false;
    
    private bool _mouseOver = false;
    public Inventory Inventory;
    public List<Action> Listeners = new();
    
    public SquadUnit()
    {
    }
    
    public SquadUnit(string unitName, Inventory inventory)
    {
        UnitName = unitName;
        Inventory = inventory;
        Texture = Globals.Database.GetTexture(unitName);
    }
    
    public override void _Ready()
    {
        AddToGroup("SquadUnit");
        
        MouseEntered += () =>
        {
            _mouseOver = true;
            UpdateModulate();
        };
        MouseExited += () =>
        {
            _mouseOver = false;
            UpdateModulate();
        };

        MouseFilter = MouseFilterEnum.Stop;
        ExpandMode = ExpandModeEnum.IgnoreSize;
        StretchMode = StretchModeEnum.KeepCentered;
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
    }

    private void UpdateModulate()
    {
        if (_mouseOver)
        {
            Modulate = Selected ? new Color(2, 2, 2) : new Color(1.75f, 1.75f, 1.75f);
        }
        else
        {
            Modulate = Selected ? new Color(1.75f, 1.75f, 1.75f) : new Color(1f, 1f, 1f);
        }
    }
    
    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);
        if (@event is not InputEventMouseButton mouseButton  
            || mouseButton.ButtonIndex != MouseButton.Left 
            || !mouseButton.Pressed
            || mouseButton.Canceled) return;

        if (mouseButton.DoubleClick)
        {
            Listeners.ForEach(listener => listener());
            GD.Print("Double clicked on " + UnitName + " unit.");
        }
        else
        {
            Selected = !Selected;
            UpdateModulate();
            GD.Print("Clicked on " + UnitName + " unit. Selected: " + Selected);
        }
    }
    
    public void SetSelected(bool selected)
    {
        Selected = selected;
        UpdateModulate();
    }
}
