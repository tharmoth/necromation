using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
            Globals.MapScene.TileMap.UpdateFogOfWar();
            
            _ownerShade.Visible = _owner == "Player";
        }
    }
    
    /**************************************************************************
     * Logic Variables                                                        *
     **************************************************************************/
    // Note: If you add more state data here make sure to serialize it in Save/Load
    public string ProvinceName { get; set; } = MapUtils.GetRandomProvinceName();
    public ImmutableList<Commander> Commanders => _commanders.ToImmutableList();
    private readonly List<Commander> _commanders = new();
    public readonly Inventory Units = new();

    private string _resource;
    public string Resource
    {
        get => _resource;
        private set
        {
            _resource = value;
            _resourceSprite.Visible = false;
            if (string.IsNullOrEmpty(_resource)) return;
            _resourceSprite.Texture = MapUtils.GetTexture(Resource);
            _resourceSprite.Scale = (Vector2.One * MapTileMap.TileSize / 4.0f) / _resourceSprite.Texture.GetSize().X;
            _resourceSprite.Visible = true;
        }
    }
    private string _owner = "Unclaimed";
    
    /**************************************************************************
     * Visuals Variables 													  *
     **************************************************************************/
    private readonly Sprite2D _flagSprite = new();
    private readonly Sprite2D _resourceSprite = new();
    private readonly Node2D _spriteHolder = new();
    private readonly Polygon2D _ownerShade = new();
    
    public Province(string resource)
    {
        Resource = resource;

        _resourceSprite.Position += (Vector2.Up + Vector2.Right) * MapTileMap.TileSize / 4.0f;
        AddChild(_resourceSprite);
        
        _flagSprite.Texture = MapUtils.GetTexture("Unclaimed Flag");
        _flagSprite.Scale = (Vector2.One * MapTileMap.TileSize / 4.0f) / _flagSprite.Texture.GetSize().X;
        var flagBottom = _flagSprite.Texture.GetHeight() * _flagSprite.Scale.Y;
        _flagSprite.Position += Vector2.Up * flagBottom / 2;
        AddChild(_flagSprite);
        
        var color = Globals.PlayerColor;
        color.A = .25f;
        _ownerShade.Color = color;
        _ownerShade.ZIndex = -1;
        _ownerShade.Polygon = GetCorners(Vector2.Zero).ToArray();
        AddChild(_ownerShade);
        
        var labelScale = .25f;
        
        // Label only cooperates if it's a child of a container.
        var panelContainer = new PanelContainer();
        panelContainer.Theme = GD.Load<Theme>("res://src/shared/gui/theme.tres");
        panelContainer.Scale = Vector2.One * labelScale;
        panelContainer.Size = Vector2.One * (MapTileMap.TileSize * 1 / labelScale * .8f);
        panelContainer.Position = Vector2.One * (-MapTileMap.TileSize / 2.0f * .8f);
        panelContainer.MouseFilter = Control.MouseFilterEnum.Ignore;
        // AddChild(panelContainer);

        var label = new Label();
        label.Text = ProvinceName;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Bottom;
        label.AutowrapMode = TextServer.AutowrapMode.Word;
        label.SizeFlagsVertical = Control.SizeFlags.ShrinkEnd;
        label.MouseFilter = Control.MouseFilterEnum.Ignore;
        label.AddThemeColorOverride("font_color", new Color(.8f, .8f, .8f, .8f));
        panelContainer.AddChild(label);

        Visible = false;
        
        _spriteHolder.YSortEnabled = true;
        AddChild(_spriteHolder);
    }

    public void OnOpen()
    {
        // This is fairly intensive so only do it when we open the map.
        // Note: this will need to be changed if we make the factory run while the map is open.
        UpdateSprite();
    }

    public override void _Ready()
    {
        base._Ready();
        //We can only set globalposition in _ready
        GlobalPosition = Globals.MapScene.TileMap.MapToGlobal(MapPosition);
    }
    
    public List<Vector2> GetCorners(Vector2 origin)
    {
        var corners = new List<Vector2>
        {
            origin + (Vector2.Up + Vector2.Left) * MapTileMap.TileSize / 2,
            origin + (Vector2.Up + Vector2.Right) * MapTileMap.TileSize / 2,
            origin + (Vector2.Down + Vector2.Right) * MapTileMap.TileSize / 2,
            origin + (Vector2.Down + Vector2.Left) * MapTileMap.TileSize / 2,
        };
        return corners;
    }
    
    private void UpdateSprite()
    {
        _spriteHolder.GetChildren().ToList().ForEach(child => child.QueueFree());
        float x = 0;
        float y = 0;
        float offset = 0;
        // Find the key with the highest value
        foreach (var commander in _commanders)
        {
            foreach (var (unitType, count) in commander.Units.Items)
            {
                _spriteHolder.Visible = true;
            
                for (var i = 0; i < count; i += 10)
                {
                    var subSprite = new Sprite2D();
                    subSprite.Texture = Database.Instance.GetTexture(unitType);
                    subSprite.Scale = Vector2.One * (MapTileMap.TileSize / 6.0f) / subSprite.Texture.GetWidth();
                    subSprite.Position = new Vector2(x + offset, y * 2) * MapTileMap.TileSize / 30.0f;
                    subSprite.Position += Vector2.Left * MapTileMap.TileSize / 6.0f;
                    subSprite.YSortEnabled = true;
                    subSprite.FlipH = commander.Team != "Player";
                    _spriteHolder.AddChild(subSprite);
                
                    x += 1;
                    if (!(x >= 10)) continue;
                    x = 0;
                    offset = offset > 0 ? 0 : .5f;
                    y += 1;
                }
            }
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
            { "Resource", Resource }
        };
        foreach (var commander in _commanders)
        {
            dict["Commander" + _commanders.IndexOf(commander)] = commander.Save();
        }

        return dict;
    }
	
    public static void Load(Godot.Collections.Dictionary<string, Variant> nodeData)
    {
        var mapPosition = new Vector2I((int)nodeData["PosX"], (int)nodeData["PosY"]);
        var province = Globals.MapScene.TileMap.GetProvence(mapPosition);
        province.Name = nodeData.TryGetValue("Name", out var name) ? name.As<string>() : province.Name;
        province.Owner = nodeData.TryGetValue("Owner", out var owner) ? owner.As<string>() : province.Owner;
        province.Resource = nodeData.TryGetValue("Resource", out var resource) ? resource.As<string>() : province.Resource;
        
        province._commanders.ForEach(commander => commander.QueueFree());
        for (int i = province._commanders.Count - 1; i >= 0; i--)
        {
            province.RemoveCommander(province._commanders[i]);
        }

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
    
    public void RemoveCommander(Commander commander)
    {
        _commanders.Remove(commander);
        UpdateSprite();
    }

    public void AddCommander(Commander commander)
    {
        commander.GlobalPosition = Globals.MapScene.TileMap.MapToGlobal(MapPosition);
        _commanders.Add(commander);
        UpdateSprite();
        
        Globals.MapScene.CallDeferred("add_child", commander);
    }
}