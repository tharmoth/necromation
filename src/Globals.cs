using System;
using System.Collections.Generic;
using Godot;
using Necromation.character;
using Necromation.gui;
using Necromation.shared.gui;

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
    
    public static Main FactoryScene => SceneManager.FactoryScene;
    public static Map MapScene => SceneManager.MapScene;
    public static Battle BattleScene => SceneManager.BattleScene;
    
    public static SceneTree Tree => SceneManager.SceneTree;

    //TODO: make this actually the current gui. Maybe as a part of scene manager?
    public static SceneGUI CurrentGUI => FactoryGUI.Instance;
}