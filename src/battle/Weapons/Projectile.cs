using System;
using Godot;

namespace Necromation.map.battle;

public partial class Projectile : Sprite2D
{
    public Vector2I MapPosition => Globals.BattleScene.TileMap.GlobalToMap(GlobalPosition);

    public const int TilesPerSecond = 15;
    private const float Speed = TilesPerSecond * BattleTileMap.TileSize;
    private readonly float ArcHeight;
    
    private readonly Vector2 _startPosition;
    private Vector2 _targetPosition;
    private readonly Vector2 _offset;
    
    private readonly float _stepScale;
    
    private float _progress;
    
    private readonly Action<Vector2I> _damage;
    
    private bool _hit = false;

    private string _type;
    private Unit _targetUnit;

    public Projectile(Vector2I startTile, Vector2I targetTile, Action<Vector2I> damage, string type, Unit target = null)
    {
        _type = type;
        _damage = damage;
        Texture = Database.Instance.GetTexture(type);
        _targetUnit = target;
        
        Scale = new Vector2(32 / (float)Texture.GetWidth(),
            32 / (float)Texture.GetHeight());

        _startPosition = Globals.BattleScene.TileMap.MapToGlobal(startTile);
        _offset = new Vector2(GD.RandRange(-BattleTileMap.TileSize / 2, BattleTileMap.TileSize / 2),
            GD.RandRange(-BattleTileMap.TileSize, BattleTileMap.TileSize));
        _targetPosition = Globals.BattleScene.TileMap.MapToGlobal(targetTile) + _offset;
        var distance = _startPosition.DistanceTo(Globals.BattleScene.TileMap.MapToGlobal(targetTile));
        _stepScale = Speed / distance;
        
        ArcHeight = distance / 4;
        
        GlobalPosition = _startPosition;
        
        if (type == "fireball")
        {
            Texture = new Texture2D();
            var particles = Database.Instance.GetParticles("fireball");
            particles.Position = new Vector2(0, 0);
            particles.ZIndex = 100;
            AddChild(particles);
        }
    }

    public override void _Process(double delta)
    {
        if (_hit) return;
        if (_targetUnit != null) _targetPosition = Globals.BattleScene.TileMap.MapToGlobal(_targetUnit.MapPosition) + _offset;
        _progress += _stepScale * (float)delta;
        _progress = Mathf.Clamp(_progress, 0, 1);

        var parabola = 1.0f - 4.0f * Mathf.Pow(_progress - 0.5f, 2);
        var nextPosition = _startPosition.Lerp(_targetPosition, _progress);
        nextPosition.Y -= parabola * ArcHeight;
        LookAt(nextPosition);
        GlobalPosition = nextPosition;
        
        // Rotation = Mathf.Atan2(_targetPosition.Y - nextPosition.Y, _targetPosition.X - nextPosition.X);
        if (_type == "Pilum") Rotation += Mathf.Pi / 4;
        
        // If the arrow has reached the target, remove it
        if (!(nextPosition.DistanceTo(_targetPosition) < 1)) return;

        _damage(MapPosition);
        Modulate = new Color(.5f, .5f, .5f);
        _hit = true;
        ZIndex = -1;
        SelfModulate = new Color(1, 1, 1, .5f);
        if (_type == "fireball")
        {
            QueueFree();
        }
    }
}