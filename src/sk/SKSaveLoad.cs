﻿using System;
using Godot;
using Necromation.interactables.interfaces;

namespace Necromation.sk;

public class SKSaveLoad
{
    // Note: This can be called from anywhere inside the tree. This function is
    // path independent.
    public static void LoadGame(Node baseNode)
    {
        if (!FileAccess.FileExists("user://savegame.save"))
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
        using var saveGame = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Read);

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

            if (Building.IsBuilding(nodeData["ItemType"].ToString())) Building.Load(nodeData);
            else Resource.Load(nodeData);

            // // Firstly, we need to create the object and add it to the tree and set its position.
            // var newObjectScene = GD.Load<PackedScene>(nodeData["Filename"].ToString());
            // var newObject = newObjectScene.Instantiate<Node>();
            // baseNode.GetNode(nodeData["Parent"].ToString()).AddChild(newObject);
            // newObject.Set(Node2D.PropertyName.Position, new Vector2((float)nodeData["PosX"], (float)nodeData["PosY"]));
            //
            // // Now we set the remaining variables.
            // foreach (var (key, value) in nodeData)
            // {
            //     if (key == "Filename" || key == "Parent" || key == "PosX" || key == "PosY")
            //     {
            //         continue;
            //     }
            //     newObject.Set(key, value);
            // }
        }
    }
    
    // Note: This can be called from anywhere inside the tree. This function is
    // path independent.
    // Go through everything in the persist category and ask them to return a
    // dict of relevant variables.
    public static void SaveGame(Node baseNode)
    {
        using var saveGame = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Write);

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
    }
}