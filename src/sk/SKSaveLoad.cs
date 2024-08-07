using System;
using Godot;
using Necromation.factory;
using Necromation.interactables.interfaces;

namespace Necromation.sk;

public class SKSaveLoad
{
    // Note: This can be called from anywhere inside the tree. This function is
    // path independent.
    public static void LoadGame(Node baseNode)
    {
        if (!FileAccess.FileExists("user://savegame.json"))
        {
            return; // Error! We don't have a save to load.
        }

        // We need to revert the game state so we're not cloning objects during loading.
        // This will vary wildly depending on the needs of a project, so take care with
        // this step.
        // For our example, we will accomplish this by deleting saveable objects.
        var saveNodes = baseNode.GetTree().GetNodesInGroup("Persist");
        foreach (Node saveNode in saveNodes)
        {
            saveNode.Free();
        }

        // Load the file line by line and process that dictionary to restore the object
        // it represents.
        using var saveGame = FileAccess.Open("user://savegame.json", FileAccess.ModeFlags.Read);

        while (saveGame.GetPosition() < saveGame.GetLength())
        {
            var jsonString = saveGame.GetLine();

            // Creates the helper class to interact with JSON
            var json = new Json();
            var parseResult = json.Parse(jsonString);
            if (parseResult != Error.Ok)
            {
                GD.Print($"JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
                continue;
            }

            // Get the data from the JSON object
            var nodeData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
            switch (nodeData["ItemType"].ToString())
            {
                case "Character":
                    Character.Load(nodeData);
                    break;
                case "BuildingManager":
                    BuildingSystem.Load(nodeData);
                    break;
                case "HotBar":
                    HotBarItemBox.Load(nodeData);
                    break;
                case "Technology":
                    Database.Load(nodeData);
                    break;
                case "Map":
                    //TODO: Redo this
                    // MapTileMap.Load(nodeData);
                    break;
                default:
                    Resource.Load(nodeData);
                    break;
            }
        }
    }
    
    // Note: This can be called from anywhere inside the tree. This function is
    // path independent.
    // Go through everything in the persist category and ask them to return a
    // dict of relevant variables.
    public static void SaveGame(Node baseNode)
    {
        using var saveGame = FileAccess.Open("user://savegame.json", FileAccess.ModeFlags.Write);

        // We need to save this first or furnace recipes will mess up on load.
        var techData = Database.Save();
        var techJson = Json.Stringify(techData);
        saveGame.StoreLine(techJson);
        
        var saveNodes = baseNode.GetTree().GetNodesInGroup("Persist");
        foreach (Node saveNode in saveNodes)
        {
            // Check the node has a save function.
            if (!saveNode.HasMethod("Save"))
            {
                GD.Print($"persistent node '{saveNode.Name}' is missing a Save() function, skipped");
                continue;
            }

            // Call the node's save function.
            var nodeData = saveNode.Call("Save");

            // Json provides a static method to serialized JSON string.
            var jsonString = Json.Stringify(nodeData);

            // Store the save dictionary as a new line in the save file.
            saveGame.StoreLine(jsonString);
        }

        var playerData = Globals.Player.Save();
        var playerJson = Json.Stringify(playerData);
        saveGame.StoreLine(playerJson);
        
        var buildingData = Locator.BuildingSystem.Save();
        var buildingJson = Json.Stringify(buildingData);
        saveGame.StoreLine(buildingJson);

        var hotBarData = HotBarItemBox.Save();
        var hotBarJson = Json.Stringify(hotBarData);
        saveGame.StoreLine(hotBarJson);

        // TODO: Fix this
        // var mapData = Globals.MapScene.TileMap.Save();
        // var mapJson = Json.Stringify(mapData);
        // saveGame.StoreLine(mapJson);
        
        MusicManager.Play("save");
    }
}