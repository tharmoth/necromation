using System;
using System.Collections.Generic;
using Godot;
using Necromation.battle;
using Necromation.factory;
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

    // Avoids having all of the C# buildings in the tree. This lets classes access the manager without the scene tree.
    public static BuildingManager BuildingManager;
    
    // Avoids having all of the C# Units in the tree. This lets classes access the manager without the scene tree.
    public static UnitManager UnitManager;

    // I think this should probably go somewhere else. Maybe associated with the player?
    public static Technology CurrentTechnology;
    public static List<Action> ResearchListeners = new();

    public static int Souls;
    
    public static FactoryScene FactoryScene => SceneManager.FactoryScene;
    public static MapScene MapScene => SceneManager.MapScene;
    public static BattleScene BattleScene => SceneManager.BattleScene;
    public static SceneTree Tree => SceneManager.SceneTree;
    
    public static Color PlayerColor = new("2e1345");
}