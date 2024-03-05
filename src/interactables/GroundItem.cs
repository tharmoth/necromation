using Godot;

namespace Necromation;

public partial class GroundItem : Node2D, BuildingTileMap.IEntity
{
    private const int Size = 16;
    public string ItemType { get; set; }
    private Sprite2D _sprite;
    private Vector2 _cachePosition;
    public Vector2 CachePosition
    {
        get => _cachePosition;
        set
        {
            _cachePosition = value;
            var transform = Transform2D.Identity.Translated(_cachePosition);
            RenderingServer.CanvasItemSetTransform(_renderingServerId, transform);
        }
    }
    
    private Texture2D _texture;
    private Rid _renderingServerId = RenderingServer.CanvasItemCreate();

    public GroundItem(string itemType)
    {
        ItemType = itemType;
    }
    
    public override void _Ready()
    {
        base._Ready();
        CachePosition = GlobalPosition;
        
        // Add the texture to the rendering server directly for performance reasons.
        RenderingServer.CanvasItemSetParent(_renderingServerId, GetCanvasItem());
        _texture = Globals.Database.GetTexture(ItemType);
        RenderingServer.CanvasItemAddTextureRect(_renderingServerId, new Rect2(CachePosition - Vector2.One * Size / 2, Vector2.One * Size), _texture.GetRid());
        var transform = Transform2D.Identity.Translated(CachePosition);
        RenderingServer.CanvasItemSetTransform(_renderingServerId, transform);
        RenderingServer.CanvasItemSetZIndex(_renderingServerId, 1);
    }
    
    public bool AddToInventory(Inventory inventory)
    {
        inventory.Insert(ItemType);
        QueueFree();
        return true;
    }
}