using System;
using Godot;

namespace Necromation.map.gui;

public partial class UnitRect : TextureRect
{
    private static Texture2D soldierTexture = GD.Load<Texture2D>("res://res/sprites/soldier.png");
    private static  Texture2D archerTexture = GD.Load<Texture2D>("res://res/sprites/archer.png");
    private static  Texture2D horseTexture = GD.Load<Texture2D>("res://res/sprites/horse.png");
    private static  Texture2D warriorTexture = GD.Load<Texture2D>("res://res/sprites/warrior.png");
    [Signal] public delegate void DoubleClickedEventHandler(UnitRect rect);
    
    public string UnitName { get; }
    public bool Selected = false;
    
    private bool _mouseOver = false;
    public Inventory Inventory;
    
    public UnitRect(string unitName, Inventory inventory)
    {
        this.UnitName = unitName;
        this.Inventory = inventory;
    }
    
    public override void _Ready()
    {
        Texture = UnitName.ToLower() switch
        {
            "soldier" => soldierTexture,
            "archer" => archerTexture,
            "horse" => horseTexture,
            "warrior" => warriorTexture,
            _ => throw new NotImplementedException()
        };
        
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
        SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        SizeFlagsVertical = SizeFlags.ShrinkCenter;
        CustomMinimumSize = new Vector2(32, 32);
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
            GD.Print("Double clicked");
            EmitSignal(SignalName.DoubleClicked, this);
        }
        else
        {
            GD.Print("Mouse clicked");
            Selected = !Selected;
            UpdateModulate();
        }
    }
    
    public void SetSelected(bool selected)
    {
        Selected = selected;
        UpdateModulate();
    }
}