﻿using System;
using System.Collections.Generic;
using Godot;
using Necromation.map.character;

namespace Necromation.map;

public partial class Province : Node2D, ITransferTarget
{
    /**************************************************************************
     * Properties                                                             *
     **************************************************************************/
    public Vector2I MapPosition => Globals.MapScene.TileMap.GetLocation(this);
    public string Owner
    {
        get => _owner;
        set
        {
            _owner = value;
            _flagSprite.Texture = MapUtils.GetTexture($"{_owner} Flag");
        }
    }
    
    /**************************************************************************
     * Logic Variables                                                        *
     **************************************************************************/
    // Note: If you add more state data here make sure to serialize it in Save/Load
    public string ProvinceName { get; } = MapUtils.GetRandomProvinceName();
    public readonly List<Commander> Commanders = new();
    public readonly Inventory Units = new();
    private string _owner = "Unclaimed";
    
    /**************************************************************************
     * Visuals Variables 													  *
     **************************************************************************/
    private Sprite2D _flagSprite = new();
    
    public Province()
    {
        _flagSprite.Texture = MapUtils.GetTexture("Unclaimed Flag");
        _flagSprite.Scale = new Vector2(0.25f, 0.25f);
        AddChild(_flagSprite);
        
        var labelScale = .25f;
        
        // Label only cooperates if it's a child of a container.
        var panelContainer = new PanelContainer();
        panelContainer.Theme = GD.Load<Theme>("res://src/shared/gui/theme.tres");
        panelContainer.Scale = Vector2.One * labelScale;
        panelContainer.Size = Vector2.One * (MapTileMap.TileSize * 1 / labelScale * .8f);
        panelContainer.Position = Vector2.One * (-MapTileMap.TileSize / 2.0f * .8f);
        AddChild(panelContainer);

        var label = new Label();
        label.Text = ProvinceName;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Bottom;
        label.AutowrapMode = TextServer.AutowrapMode.Word;
        label.SizeFlagsVertical = Control.SizeFlags.ShrinkEnd;
        label.AddThemeColorOverride("font_color", new Color(.8f, .8f, .8f, .8f));
        panelContainer.AddChild(label);

    }

    public override void _Ready()
    {
        base._Ready();
        //We can only set globalposition in _ready
        GlobalPosition = Globals.MapScene.TileMap.MapToGlobal(MapPosition);
        _flagSprite.GlobalPosition = Globals.MapScene.TileMap.MapToGlobal(Globals.MapScene.TileMap.GetLocation(this));
        _flagSprite.GlobalPosition -= Vector2.One * MapTileMap.TileSize / 4.0f;
    }
    
    /**************************************************************************
     * Initialization                                                         *
     **************************************************************************/
    public void Init(string team)
    {
        Owner = team;
        Globals.MapScene.CallDeferred("add_child", this);
        
        if (team == "Player" && Globals.FactoryScene != null) return;
        
		
        var meleeCommander = new Commander(this, team);
        var meleeCommander2 = new Commander(this, team);
        var meleeCommander3 = new Commander(this, team);
        var rangedCommander = new Commander(this, team);
        rangedCommander.SpawnLocation = new Vector2I(30, 25);
		
        var provinceLocation = MapPosition;
        var dis = Mathf.Abs(provinceLocation.X - MapScene.FactoryPosition.X) + Mathf.Abs(provinceLocation.Y - MapScene.FactoryPosition.Y);
		
        switch (dis)
        {
            case 0:
                break;
            case 1:
                meleeCommander.Units.Insert("Rabble", 10);
                break;
            case 2:
                meleeCommander.Units.Insert("Infantry", 10);
                rangedCommander.Units.Insert("Archer", 10);
                break;
            case 3:
                meleeCommander.Units.Insert("Infantry", 50);
                rangedCommander.Units.Insert("Archer", 25);
                break;
            case 4:
                meleeCommander.Units.Insert("Infantry", 100);
                rangedCommander.Units.Insert("Archer", 50);
                break;
            case 5:
                meleeCommander.Units.Insert("Barbarian", 200);
                meleeCommander2.Units.Insert("Heavy Infantry", 50);
                rangedCommander.Units.Insert("Archer", 100);
                break;
            case 6:
                meleeCommander.Units.Insert("Elite Infantry", 200);
                meleeCommander2.Units.Insert("Barbarian", 200);
                meleeCommander3.Units.Insert("Barbarian", 200);
                break;
        }
    }
    
    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item, int count = 1) => Units.CanAcceptItems(item, count);
    public bool CanAcceptItemsInserter(string item, int count = 1) => Units.CanAcceptItemsInserter(item, count);
    public void Insert(string item, int count = 1) => Units.Insert(item, count);
    public bool Remove(string item, int count = 1) => Units.Remove(item, count);
    public string GetFirstItem() => Units.GetFirstItem();
    public List<string> GetItems() => Units.GetItems();
    public List<Inventory> GetInventories() => Units.GetInventories();
    #endregion
    
    #region Save/Load
    /******************************************************************
     * Save/Load                                                      *
     ******************************************************************/
    public virtual Godot.Collections.Dictionary<string, Variant> Save()
    {
        var dict =  new Godot.Collections.Dictionary<string, Variant>()
        {
            { "ItemType", "Province" },
            { "Name", ProvinceName },
            { "PosX", MapPosition.X }, // Vector2 is not supported by JSON
            { "PosY", MapPosition.Y },
            { "Owner", Owner },
        };
        foreach (var commander in Commanders)
        {
            dict["Commander" + Commanders.IndexOf(commander)] = commander.Save();
        }

        return dict;
    }
	
    public static void Load(Godot.Collections.Dictionary<string, Variant> nodeData)
    {
        var mapPosition = new Vector2I((int)nodeData["PosX"], (int)nodeData["PosY"]);
        var province = Globals.MapScene.TileMap.GetProvence(mapPosition);
        province.Name = nodeData["Name"].ToString();
        province.Owner = nodeData["Owner"].ToString();;
        province.Commanders.Clear();

        int index = 0;
        while (nodeData.ContainsKey("Commander" + index))
        {
            var data = (Godot.Collections.Dictionary<string, Variant>) nodeData["Commander" + index];
            Commander.Load(data);
            index++;
        }
        
        // We need to load the province into the Factory Scene as well if it has been conquered by the player.
        if (province.Owner == "Player")
        {
            Globals.FactoryScene.TileMap.AddProvence(mapPosition, false);
        }
    }
    #endregion

    public void AddCommander(Commander commander)
    {
        commander.GlobalPosition = Globals.MapScene.TileMap.MapToGlobal(MapPosition);
        Commanders.Add(commander);
        Globals.MapScene.CallDeferred("add_child", commander);
    }
}