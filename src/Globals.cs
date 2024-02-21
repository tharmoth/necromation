using System;
using System.Collections.Generic;
using Godot;
using Necromation.character;
using Necromation.gui;

namespace Necromation;

public class Globals
{ 
    public static Character Player;
    public static BuildingTileMap TileMap;
    public static Inventory PlayerInventory;
    private static Database _database;
    public static Database Database => _database ??= new Database();
    public static Technology CurrentTechnology = Database.Technologies[0];
    public static List<Action> ResearchListeners = new();
    public static Camera2D Camera => Player.GetNode<Camera2D>("Camera2D");

    public static Node FactoryScene;
    public static Node MapScene;
    public static Node BattleScene;
}