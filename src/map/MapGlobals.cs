using System;
using System.Collections.Generic;
using Godot;
using Necromation.map.character;

namespace Necromation.map;

public class MapGlobals
{
    public static readonly List<Action> TurnListeners = new() {() => SelectedCommander = null} ;
    public static readonly List<Action> UpdateListeners = new();
    
    public static MapTileMap TileMap;
    public static Sprite2D SelectedSprite;
    public static Province SelectedProvince;
    public static Commander SelectedCommander;

    public static Vector2I FactoryPosition => new(4, 2);
}