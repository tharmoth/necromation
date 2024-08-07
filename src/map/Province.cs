using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using Necromation.map.character;

namespace Necromation.map;

public partial class Province : Node2D
{
    /**************************************************************************
     * Public Variables                                                       *
     **************************************************************************/
    public required List<Vector2> Vertices { get; init; }
    public required Biome Biome { get; init; }
    public required Vector2 PositionPercent { get; init; }
    public readonly string ProvinceName  = MapUtils.GetRandomProvinceName();
    
    /**************************************************************************
	 * Private Variables                                                      *
	 **************************************************************************/
    // Visuals
    private readonly Sprite2D _flagSprite = new();
    private readonly Sprite2D _resourceSprite = new();
    private readonly Node2D _spriteHolder = new();
    
    // Logic
    private string _resource;
    private string _owner = "Enemy";
    
    /**************************************************************************
     * Godot Methods                                                          *
     **************************************************************************/
    public override void _Ready()
    {
        base._Ready();

        if (Biome != Biome.Ocean)
        {
            var location = Actor.ActorBuilder.Create().AsMapLocation(this).Build();
            CallDeferred("add_sibling", location);
            
            _resourceSprite.Position += (Vector2.Up + Vector2.Right) * MapScene.MapCellSize / 4.0f;
            AddChild(_resourceSprite);
        
            _flagSprite.Texture = MapUtils.GetTexture("Unclaimed Flag");
            _flagSprite.Scale = (Vector2.One * MapScene.MapCellSize / 4.0f) / _flagSprite.Texture.GetSize().X;
            var flagBottom = _flagSprite.Texture.GetHeight() * _flagSprite.Scale.Y;
            _flagSprite.Position += Vector2.Up * flagBottom / 2;
            AddChild(_flagSprite);
            
            var labelScale = .25f;
        
            // Label only cooperates if it's a child of a container.
            var panelContainer = new PanelContainer();
            panelContainer.Theme = GD.Load<Theme>("res://src/shared/gui/theme.tres");
            panelContainer.Scale = Vector2.One * labelScale;
            panelContainer.Size = Vector2.One * (MapScene.MapCellSize * 1 / labelScale * .8f);
            panelContainer.Position = Vector2.One * (-MapScene.MapCellSize / 2.0f * .8f);
            panelContainer.MouseFilter = Control.MouseFilterEnum.Ignore;
            AddChild(panelContainer);

            var label = new Label();
            label.Text = ProvinceName;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Bottom;
            label.AutowrapMode = TextServer.AutowrapMode.Word;
            label.SizeFlagsVertical = Control.SizeFlags.ShrinkEnd;
            label.MouseFilter = Control.MouseFilterEnum.Ignore;
            label.AddThemeColorOverride("font_color", new Color(.8f, .8f, .8f, .8f));
            panelContainer.AddChild(label);

            Visible = true;
        
            _spriteHolder.YSortEnabled = true;
            AddChild(_spriteHolder);
        }
        
        var polygon = new Polygon2D()
        {
            Polygon = Vertices.Select(vert => vert - Position).ToArray(),
            Color = Biome switch
            {
                Biome.Ocean => Utils.OceanColor,
                Biome.Grassland => Utils.LandColor,
                _ => new Color(1.0f, 1.0f, 1.0f)
            },
            ZIndex = -100
        };
        AddChild(polygon);
        
        var line = new Line2D
            { 
                Points = polygon.Polygon, 
                Width = 2, 
                DefaultColor = Colors.Black, 
                ZIndex = -99
            };
        AddChild(line);
    }
    
    /**************************************************************************
     * Public Methods                                                         *
     **************************************************************************/
    public string Owner
    {
        get => _owner;
        set
        {
            _owner = value;
            _flagSprite.Texture = MapUtils.GetTexture($"{_owner} Flag");
            // Globals.MapScene.TileMap.UpdateFogOfWar();
        }
    }
    
    public required string Resource
    {
        get => _resource;
        init
        {
            _resource = value;
            _resourceSprite.Visible = false;
            if (string.IsNullOrEmpty(_resource)) return;
            _resourceSprite.Texture = MapUtils.GetTexture(Resource);
            _resourceSprite.Scale = (Vector2.One * MapScene.MapCellSize / 4.0f) / _resourceSprite.Texture.GetSize().X;
            _resourceSprite.Visible = true;
        }
    }


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
            // { "PosX", MapPosition.X }, // Vector2 is not supported by JSON
            // { "PosY", MapPosition.Y },
            { "Owner", Owner },
            { "Resource", Resource }
        };
        
        return dict;
    }
	
    public static void Load(Godot.Collections.Dictionary<string, Variant> nodeData)
    {
        var mapPosition = new Vector2I((int)nodeData["PosX"], (int)nodeData["PosY"]);
        // var province = Globals.MapScene.TileMap.GetProvence(mapPosition);
        // province.Name = nodeData.TryGetValue("Name", out var name) ? name.As<string>() : province.Name;
        // province.Owner = nodeData.TryGetValue("Owner", out var owner) ? owner.As<string>() : province.Owner;
        // province.Resource = nodeData.TryGetValue("Resource", out var resource) ? resource.As<string>() : province.Resource;
        //
        // province._commanders.ForEach(commander => commander.QueueFree());
        // for (int i = province._commanders.Count - 1; i >= 0; i--)
        // {
        //     province.RemoveCommander(province._commanders[i]);
        // }

        int index = 0;
        while (nodeData.ContainsKey("Commander" + index))
        {
            var data = (Godot.Collections.Dictionary<string, Variant>) nodeData["Commander" + index];
            Commander.Load(data);
            index++;
        }
        
        // We need to load the province into the Factory Scene as well if it has been conquered by the player.
        // if (province.Owner == "Player")
        // {
        //     Globals.FactoryScene.TileMap.AddProvence(mapPosition, false);
        // }
    }
    #endregion
}