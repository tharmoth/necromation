using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation;
using Necromation.sk;

public class Pylon : Building
{
    /**************************************************************************
     * Constants                                                              *
     **************************************************************************/
    private const int LineWidth = 2;
    
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

    /**************************************************************************
     * Godot Methods                                                          *
     **************************************************************************/
    public override void _Ready()
    {
        base._Ready();
        BaseNode.CallDeferred("add_child", _colorNode);
        _colorNode.Draw += DoDraw;
    }

    /**************************************************************************
     * Public Methods                                                         *
     **************************************************************************/
    public void Update()
    {
        Links.Clear();
        Sources.Clear();
        Consumers.Clear();
        
        Globals.FactoryScene.TileMap.GetEntitiesWithinRadius(MapPosition, 10)
            .ToList()
            .ForEach(building =>
            {
                switch (building)
                {
                    case Pylon pylon:
                        Links.Add(pylon);
                        break;
                    case Manaforge manaforge:
                        Sources.Add(manaforge);
                        break;
                    case Assembler assembler:
                        Consumers.Add(assembler);
                        break;
                }
            });
       
        _colorNode.QueueRedraw();
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
            _colorNode.DrawLine(start, end, Utils.ManaColor, LineWidth);
        }
    }
}