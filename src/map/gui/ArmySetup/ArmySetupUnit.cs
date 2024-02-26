using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class ArmySetupUnit : TextureRect
{
	public string UnitName { get; }
    public bool Selected = false;
    
    private bool _mouseOver = false;
    public Inventory Inventory;
    public List<Action> Listeners = new();
    
    public ArmySetupUnit()
    {
    }
    
    public ArmySetupUnit(string unitName, Inventory inventory)
    {
        UnitName = unitName;
        Inventory = inventory;
        Texture = Globals.Database.GetTexture(unitName);
    }
    
    public override void _Ready()
    {
        AddToGroup("ArmySetupUnit");
        
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
            if (Input.IsKeyPressed(Key.Shift))
            {
                var unit = GetTree().GetNodesInGroup("LastClickedUnit").OfType<ArmySetupUnit>().FirstOrDefault();
                if (unit == null || unit.GetParent() != GetParent()) return;
                var parentUnits = GetParent().GetChildren().OfType<ArmySetupUnit>().ToList();
                var startIndex = parentUnits.ToList().IndexOf(unit);
                var endIndex = parentUnits.ToList().IndexOf(this);
                var range = parentUnits.ToList().GetRange(Math.Min(startIndex, endIndex), Math.Abs(startIndex - endIndex) + 1);
                range.ToList().ForEach(unit => unit.SetSelected(true));
            }
            else
            {
                Selected = !Selected;
                UpdateModulate();
                GD.Print("Clicked on " + UnitName + " unit. Selected: " + Selected);
                GetTree().GetNodesInGroup("LastClickedUnit").ToList().ForEach(node => node.RemoveFromGroup("LastClickedUnit"));
                AddToGroup("LastClickedUnit");
            }
        }
    }
    
    public void SetSelected(bool selected)
    {
        Selected = selected;
        UpdateModulate();
    }
}
