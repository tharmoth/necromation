using Godot;

namespace Necromation.map.battle;

public partial class Arrow : Sprite2D
{
    public Vector2I MapPosition => Globals.BattleScene.TileMap.GlobalToMap(GlobalPosition);
    
    private const float Speed = 800;
    private const float ArcHeight = 50;
    
    private readonly Vector2 _startPosition;
    private readonly Vector2 _targetPosition;
    
    private readonly float _stepScale;
    private readonly int _damage;
    
    private float _progress;
    
    private static readonly Texture2D _texture2D = GD.Load<Texture2D>("res://res/sprites/Arrow.png");

    public Arrow(Vector2I targetTile, Vector2I startTile, int damage = 5)
    {
        _damage = damage;
        Texture = _texture2D;
        Scale = new Vector2(0.25f, 0.25f);
        _startPosition = Globals.BattleScene.TileMap.MapToGlobal(startTile);;
        _targetPosition = Globals.BattleScene.TileMap.MapToGlobal(targetTile);
        var distance = _startPosition.DistanceTo(Globals.BattleScene.TileMap.MapToGlobal(targetTile));
        _stepScale = Speed / distance;
        
        GlobalPosition = _startPosition;
    }

    public override void _Process(double delta)
    {
        _progress += _stepScale * (float)delta;
        _progress = Mathf.Clamp(_progress, 0, 1);
        var parabola = 1.0f - 4.0f * Mathf.Pow(_progress - 0.5f, 2);
        var nextPosition = _startPosition.Lerp(_targetPosition, _progress);
         nextPosition.Y -= parabola * ArcHeight;
        GlobalPosition = nextPosition;
        
        Rotation = Mathf.Atan2(_targetPosition.Y - nextPosition.Y, _targetPosition.X - nextPosition.X);
        
        // If the arrow has reached the target, remove it
        if (!(nextPosition.DistanceTo(_targetPosition) < 10)) return;

        var hit = Globals.BattleScene.TileMap.GetEntity(MapPosition, BattleTileMap.Unit);
        if (hit is Unit unit)
        {
            unit.Damage(_damage);
        }
        QueueFree();
    }
}