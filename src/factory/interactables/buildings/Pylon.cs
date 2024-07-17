using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation;
using Necromation.components.animation;
using Necromation.sk;

public class Pylon : Building
{
    /**************************************************************************
     * Constants                                                              *
     **************************************************************************/
    public const int LineWidth = 1;
    
    /**************************************************************************
     * Building Implementation                                                *
     **************************************************************************/
    public override Vector2I BuildingSize => Vector2I.One;
    public override string ItemType => "Pylon";
    
    /**************************************************************************
     * Public Variables                                                       *
     **************************************************************************/
    public readonly HashSet<Pylon> Links = new();
    public readonly HashSet<IPowerSource> Sources = new();
    public readonly HashSet<IPowerConsumer> Consumers = new();

    /**************************************************************************
     * Private Variables                                                       *
     **************************************************************************/
    private readonly Node2D _colorNode = new () {ZIndex = 100};
    private readonly Polygon2D _powerRangePolygon = GetPowerRangePolygon();

    /**************************************************************************
     * Godot Methods                                                          *
     **************************************************************************/
    public override void _Ready()
    {
        base._Ready();
        BaseNode.CallDeferred("add_child", _colorNode);
        _colorNode.Draw += DoDraw;

        BaseNode.CallDeferred("add_child", _powerRangePolygon);
    }

    /**************************************************************************
     * Public Methods                                                         *
     **************************************************************************/
    public void Update(Vector2I mapPosition)
    {
        Links.Clear();
        Sources.Clear();
        Consumers.Clear();
        
        Globals.FactoryScene.TileMap.GetEntitiesWithinRadius(mapPosition, 10)
            .OfType<Pylon>()
            .ToList()
            .ForEach(pylon => Links.Add(pylon));
        
        var powerRect = new Rect2I(mapPosition.X - 3, mapPosition.Y - 3, 7, 7);
        Globals.FactoryScene.TileMap.GetEntitiesWithinRect(powerRect)
            .OfType<Building>()
            .ToList()
            .ForEach(building =>
            {
                var source = building.GetComponent<IPowerSource>();
                if (source != null)
                {
                    Sources.Add(source);
                }
                
                var consumer = building.GetComponent<IPowerConsumer>();
                if (consumer != null)
                {
                    Consumers.Add(consumer);
                }
            });
       
        _colorNode.QueueRedraw();
    }
    
    public bool ShowRange
    {
        set => _powerRangePolygon.Visible = value;
    }

    public static Polygon2D GetPowerRangePolygon()
    {
        var color = Utils.ManaColor;
        color.A = .2f;
        var rectStart = -3 * FactoryTileMap.TileSize - FactoryTileMap.TileSize / 2;
        var rectSize = 7 * FactoryTileMap.TileSize;
        var powerRect = new Rect2I(rectStart, rectStart, rectSize, rectSize);
        return new Polygon2D 
        {
            Color = color,
            Polygon =
            [
                new Vector2(powerRect.Position.X, powerRect.Position.Y),
                new Vector2(powerRect.Position.X + powerRect.Size.X, powerRect.Position.Y),
                new Vector2(powerRect.Position.X + powerRect.Size.X, powerRect.Position.Y + powerRect.Size.Y),
                new Vector2(powerRect.Position.X, powerRect.Position.Y + powerRect.Size.Y)
            ],
            ZIndex = -1
        };
    }

    /**************************************************************************
     * Private Methods                                                        *
     **************************************************************************/
    private void DoDraw()
    {
        var buildings = Links;
            // .Union(Sources.OfType<Building>())
            // .Union(Consumers.OfType<Building>());
        
        foreach (var link in buildings)
        {
            var start = Vector2.Zero;
            var end = _colorNode.ToLocal(link.GlobalPosition);
            _colorNode.DrawLine(start, end, Utils.ManaColor, LineWidth, true);
        }

        // var color = Utils.ManaColor;
        // color.A = .2f;
        //
        // var rectStart = -3 * FactoryTileMap.TileSize - FactoryTileMap.TileSize / 2;
        // var rectSize = 7 * FactoryTileMap.TileSize;
        // var powerRect = new Rect2I(rectStart, rectStart, rectSize, rectSize);
        // _colorNode.DrawRect(powerRect, color);
    }
}