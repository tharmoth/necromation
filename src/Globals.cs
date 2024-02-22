using System;
using System.Collections.Generic;
using Godot;
using Necromation.character;
using Necromation.gui;

namespace Necromation;

public class Globals
{ 
    // I'd like to not have this reference to the player's inventory here, but I'm not sure how to avoid it
    // The gui needs to know about the player's inventory and i'm not sure how to inject it yet.
    public static Inventory PlayerInventory;
    
    // I'd like to not have this reference to the player here, but I'm not sure how to avoid it
    // The gui puts items in the players hands. Belts move the player.
    public static Character Player;
    
    // I don't mind having this here. Or a singleton referance. I'm not sure what the best practice is. But this is
    // how most of my code communicates.
    public static BuildingTileMap TileMap;

    // I don't mind having the database here. I think it's a valid singleton.
    private static Database _database;
    public static Database Database => _database ??= new Database();
    
    // I think this should probably go somewhere else. Maybe associated with the player?
    public static Technology CurrentTechnology;
    public static List<Action> ResearchListeners = new();
    
    // I think these should go in some sort of scene manager maybe?
    public static Camera2D FactoryCamera => Player.GetNode<Camera2D>("Camera2D");
    public static Camera2D MapCamera;
    public static Camera2D BattleCamera;

    public static Node FactoryScene;
    public static Node MapScene;
    public static Node BattleScene;
}