using System.Collections.Generic;
using Godot;
using Necromation.map.gui;

namespace Necromation.map.character;

public partial class Commander : Node2D, Inserter.ITransferTarget
{
    public string Name { get; } = MapUtils.GetRandomCommanderName();
    public readonly Inventory Units = new();
    private readonly Line2D _line = new();
    private readonly Sprite2D _sprite = new();
    
    private Vector2I _targetLocation;
    private Province _province;

    public Commander(Province province)
    {
        _province = province;
        _sprite.Texture = GD.Load<Texture2D>("res://res/sprites/player.png");
        _sprite.Scale = new Vector2(0.25f, 0.25f);
        AddChild(_sprite);
    }
    
    public override void _Ready()
    {
        base._Ready();
        GetTree().Root.CallDeferred("add_child", _line);
        
        GlobalPosition = MapGlobals.TileMap.MapToGlobal(MapGlobals.TileMap.GetLocation(_province));
        _targetLocation = MapGlobals.TileMap.GetLocation(_province);
        MapGlobals.TurnListeners.Add(OnTurnEnd);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        
        if (MapGlobals.SelectedCommander != this) return;

        if (!Input.IsActionJustPressed("right_click")) return;

        _targetLocation = MapGlobals.TileMap.GlobalToMap(GetGlobalMousePosition());

        var provence = MapGlobals.TileMap.GetProvence(_targetLocation);
        if (provence == null) return;

        _line.Points = new [] {GlobalPosition, MapGlobals.TileMap.MapToGlobal(_targetLocation)};
    }
    
    public void OnTurnEnd()
    {
        _line.Points = new Vector2[] {};
        if (_targetLocation == MapGlobals.TileMap.GetLocation(_province)) return;
        GlobalPosition = MapGlobals.TileMap.MapToGlobal(_targetLocation);
        _province?.Commanders.Remove(this);
        _province = MapGlobals.TileMap.GetProvence(_targetLocation);
        if (_province == null) return;
        _province.Commanders.Add(this);
        GlobalPosition = MapGlobals.TileMap.MapToGlobal(MapGlobals.TileMap.GetLocation(_province));
    }

    public Inventory GetInputInventory()
    {
        return Units;
    }

    public Inventory GetOutputInventory()
    {
        return Units;
    }

    public bool CanAcceptItem(string item)
    {
        return true;
    }
}