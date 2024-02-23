using Godot;

namespace Necromation.map.battle;

public partial class Arrow : Sprite2D
{
    Vector2I TargetTile { get; set; }
    private float _velocity = 100;
    
    public Arrow(Vector2I targetTile)
    {
        TargetTile = targetTile;
        Texture = GD.Load<Texture2D>("res://res/sprites/Arrow.png");
        Scale = new Vector2(0.25f, 0.25f);

    }

    public override void _PhysicsProcess(double delta)
    {
        var targetPosition = Globals.BattleScene.TileMap.MapToGlobal(TargetTile);
        var currnetPosition = GlobalPosition;
        var direction = (targetPosition - currnetPosition).Normalized();
        GlobalPosition += direction * _velocity * (float)delta;
        
        // If the arrow has reached the target, remove it
        if (!(GlobalPosition.DistanceTo(targetPosition) < 10)) return;
        
        var hit = Globals.BattleScene.TileMap.GetEntities(TargetTile, BattleTileMap.Unit);
        if (hit is Unit unit)
        {
            unit.Damage(5);
        }
        QueueFree();
    }
}