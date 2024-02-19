﻿using System.Collections.Generic;
using Godot;

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
        return GetRandomLine("res://res/data/Names.txt", _commanderNames);
    }
    
    private static string GetRandomLine(string filepath, List<string> usedNames)
    {
        // Load in the file from res/data/ProvenceNames.txt and return a random line
        string name;
        
        using var file = FileAccess.Open(filepath, FileAccess.ModeFlags.Read);
        var text = file.GetAsText();
        
        var lines = text.Split("\n");
            
        while (true)
        {
            name = lines[GD.RandRange(0, lines.Length - 1)].TrimEnd('\r', '\n');
            if (!usedNames.Contains(name) || usedNames.Count == lines.Length - 1)
            {
                break;
            }
        }

        usedNames.Add(name);
        
        return name;
    }
}