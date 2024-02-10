using Godot;
using Necromation.character;

namespace Necromation;

public class Globals
{
    // private static Globals _instance;
    // public static Globals Instance => _instance ??= new Globals();

    public static Character Player;
    public static BuildingTileMap TileMap;
    public static Inventory PlayerInventory;
    private static Database _database;
    public static Database Database => _database ??= new Database();
}