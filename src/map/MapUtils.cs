﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using FileAccess = Godot.FileAccess;

namespace Necromation.map;

public class MapUtils
{
    private static List<string> _provinceNames = new();
    private static List<string> _commanderNames = new();
    
    public static string GetRandomProvinceName()
    {
        return GetRandomLine("res://res/data/ProvinceNames.txt", _provinceNames);
    }
    
    public static string GetRandomCommanderName()
    {
        return GetRandomLine("res://res/data/Names2.txt", _commanderNames);
    }
    
    private static string GetRandomLine(string filepath, List<string> usedNames)
    {
        // Load in the file from res/data/ProvenceNames.txt and return a random line

        using var file = FileAccess.Open(filepath, FileAccess.ModeFlags.Read);
        var text = file.GetAsText();
        
        var lines = text.Split("\n");

        var name = lines.Select(lineName => lineName.TrimEnd('\r', '\n'))
            .Where(lineName => !usedNames.Contains(lineName))
            .MinBy(_ => GD.RandRange(0, 1)) ?? lines[GD.RandRange(0, lines.Length - 1)].TrimEnd('\r', '\n');

        usedNames.Add(name);
        
        return name;
    }
    
    public static Texture2D GetTexture(string name)
    {
        var path = "res://res/sprites/" + name + ".png";
        var texture = GD.Load<Texture2D>(path);
        if (texture == null) throw new FileNotFoundException(path);
        return texture;
    }
}