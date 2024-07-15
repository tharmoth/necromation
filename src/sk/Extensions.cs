using System.Collections.Generic;
using System.Linq;
using Godot;

public static class Extensions
{
    public static Vector2I GlobalToMap(this TileMap tileMap, Vector2 point)
    {
        return tileMap.LocalToMap(tileMap.ToLocal(point));
    }
    
    public static Vector2 MapToGlobal(this TileMap tileMap, Vector2I point)
    {
        var local = tileMap.MapToLocal(point);
        var global = tileMap.ToGlobal(local);
        return global;
    }
    
    public static Vector2 ToMap(this TileMap tileMap, Vector2 point)
    {
        return tileMap.MapToGlobal(tileMap.GlobalToMap(point));
    }

    public static void MoveToBack(this Node node)
    {
        node.GetParent().MoveChild(node, 0);
    }
    
    public static Vector2 Lerp(this Vector2 a, Vector2 b, float t)
    {
        return a + (b - a) * t;
    }

    public static void Clear(this Container container)
    {
        container.GetChildren().ToList().ForEach(child => child.QueueFree());
    }

    public static void FreeNodes(this List<Node> list)
    {
        list.ForEach(node => node.QueueFree());
    }
    
    public static void Unparent(this Node node)
    {
        node.GetParent()?.RemoveChild(node);
    }
    
    // Adjusts the position of the popup so that it is always visible on the screen
    public static void SnapToScreen(this Control node)
    {
        node.ResetSize();
		
        // Viewport can sometimes be null when 
        // var viewport = node.GetViewport();
        // if (viewport == null) return;
		
        node.GlobalPosition = node.GetViewport().GetMousePosition() + new Vector2(40, 0);
		
        // Ensure the PopupMenu is not partially off-screen
        var screenSize = node.GetViewportRect().Size;
		
        // Check if the PopupMenu exceeds the right edge of the screen move it to the left of the cursor
        if (node.GlobalPosition.X + node.Size.X > screenSize.X)
        {
            node.GlobalPosition = new Vector2(node.GetViewport().GetMousePosition().X - node.Size.X - 40, node.GlobalPosition.Y);
        }
		
        // Check if the PopupMenu exceeds the bottom edge of the screenmove it to the top of the cursor
        if (node.GlobalPosition.Y + node.Size.Y > screenSize.Y)
        {
            node.GlobalPosition = new Vector2(node.GlobalPosition.X, screenSize.Y - node.Size.Y);
        }
    }
    
    public static T GetRandom<T>(this List<T> list)
    {
        return list.ElementAt((int) (GD.Randi() % (list.Count - 1)));
    }
    
    
    public static void ScaleToSize(this Sprite2D sprite, Vector2 size)
    {
        sprite.Scale = Vector2.One * size / Mathf.Max(sprite.Texture.GetSize().X, sprite.Texture.GetSize().Y);
    }
}