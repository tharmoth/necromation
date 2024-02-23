﻿using System.Collections.Generic;
using Godot;

namespace Necromation.map.character;

public partial class Commander : Node2D, ITransferTarget
{
    public string Name { get; } = MapUtils.GetRandomCommanderName();
    public string Team { get; set; }

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
        Globals.MapScene.CallDeferred("add_child", _line);
        
        GlobalPosition = MapGlobals.TileMap.MapToGlobal(MapGlobals.TileMap.GetLocation(_province));
        _targetLocation = MapGlobals.TileMap.GetLocation(_province);
        MapGlobals.TurnListeners.Add(OnTurnEnd);
        
        if (Team != "Player") _sprite.Visible = false;
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

        if (_province.Owner != Team) ;
        _province.Owner = "Player";
    }
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item, int count = 1) => Units.CanAcceptItems(item, count);
    public void Insert(string item, int count = 1) => Units.Insert(item, count);
    public bool Remove(string item, int count = 1) => Units.Remove(item, count);
    public string GetFirstItem() => Units.GetFirstItem();
    public List<string> GetItems() => Units.GetItems();
    public List<Inventory> GetInventories() => Units.GetInventories();
    #endregion


}