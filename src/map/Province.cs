using System.Collections.Generic;
using Godot;
using Necromation.character;
using Necromation.map.character;

namespace Necromation.map;

public partial class Province : Node2D, ITransferTarget
{
    public string Name { get; } = MapUtils.GetRandomProvinceName();
    public readonly Dictionary<string, int> RecruitQueue = new();
    public readonly Inventory Units = new();
    public readonly List<Commander> Commanders = new();
    private Sprite2D _flagSprite = new();
    private string _owner = "Unclaimed";
    public string Owner
    {
        get => _owner;
        set
        {
            _owner = value;
            _flagSprite.Texture = MapUtils.GetTexture($"{_owner} Flag");
        }
    }
    
    public Province()
    {
        MapGlobals.TurnListeners.Add(Recruit);
        _flagSprite.Texture = MapUtils.GetTexture("Unclaimed Flag");
        _flagSprite.Scale = new Vector2(0.25f, 0.25f);
        AddChild(_flagSprite);
    }

    public override void _Ready()
    {
        base._Ready();
        _flagSprite.GlobalPosition = MapGlobals.TileMap.MapToGlobal(MapGlobals.TileMap.GetLocation(this));
        _flagSprite.GlobalPosition -= Vector2.One * MapTileMap.TileSize / 4.0f;
    }

    public void Recruit(string type)
    {
        RecruitQueue.TryGetValue(type, out var currentCount);
        RecruitQueue[type] = currentCount + 1;
    }
    
    private void Recruit()
    {
        foreach (var (type, count) in RecruitQueue)
        {
            if (type == "commander")
            {
                for (var i = 0; i < count; i++)
                {
                    var commander = new Commander(this);
                    Commanders.Add(commander);
                    Globals.MapScene.AddChild(commander);
                }
            }
            else
            {
                Units.Insert(type, count);
            }
        }
        RecruitQueue.Clear();
    }

    #region ITransferTarget Implementation
    /**************************************************************************
     * ITransferTarget Methods                                                *
     **************************************************************************/
    public bool CanAcceptItems(string item, int count = 1) => Units.CanAcceptItems(item, count);
    public void Insert(string item, int count = 1) => Units.Insert(item, count);
    public bool Remove(string item, int count = 1) => Units.Remove(item, count);
    public string GetFirstItem() => Units.GetFirstItem();
    public List<string> GetItems() => Units.GetItems();
    public List<Inventory> GetInventories() => Units.GetInventories();
    #endregion
}