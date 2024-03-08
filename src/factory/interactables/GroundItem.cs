using Godot;

namespace Necromation;

public class GroundItem
{
    // String representation of the item type. Used to look up the texture in the database.
    public string ItemType { get; set; }
    
    // Index of the item on it's parent transport belt. Allows the item to bypass processing if it has already reached.
    // It's target location.
    public int CacheIndex = -1;
    
    // Hand backed version of Node2D.GlobalPosition. Used to update the rendering server.
    private Vector2 _globalPosition;
    public Vector2 GlobalPosition
    {
        get => _globalPosition;
        set
        {
            _globalPosition = value;

            var transform = Transform2D.Identity.Translated(_globalPosition);
            RenderingServer.CanvasItemSetTransform(_renderingServerId, transform);
        }
    }
    
    // The size in pixels that the texture is to be drawn at.
    private const int Size = 16;
    
    // A referance to the texture that is to be drawn in the rendering server.
    // Effectively our sprite.
    private Rid _renderingServerId = RenderingServer.CanvasItemCreate();

    public GroundItem(string itemType)
    {
        ItemType = itemType;
        
        GlobalPosition = Vector2.Zero;
        
        // Add the texture to the rendering server directly for performance reasons.
        RenderingServer.CanvasItemSetParent(_renderingServerId, Globals.FactoryScene.GroundItemHolder.GetCanvasItem());
        var texture = Database.Instance.GetTexture(ItemType);
        RenderingServer.CanvasItemAddTextureRect(_renderingServerId, new Rect2(GlobalPosition - Vector2.One * Size / 2, Vector2.One * Size), texture.GetRid());
        var transform = Transform2D.Identity.Translated(GlobalPosition);
        RenderingServer.CanvasItemSetTransform(_renderingServerId, transform);
        RenderingServer.CanvasItemSetZIndex(_renderingServerId, 1);
    }

    // Removes the texture from the rendering server.
    public void Free()
    {
        RenderingServer.CanvasItemClear(_renderingServerId);
    }
}