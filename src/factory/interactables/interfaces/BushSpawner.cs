using Godot;

namespace Necromation.factory.interactables.interfaces;

public partial class BushSpawner : Node2D
{
    public int Radius = 1000;
    public float Threshold = 0.98f;
    
    public override void _Ready()
    {
        base._Ready();
        YSortEnabled = true;
        CallDeferred("Spawn");
    }

    private void Spawn()
    {
        for (var x = -Radius; x < Radius; x += 64)
        {
            for (var y = -Radius; y < Radius; y += 64)
            {
                if (new RandomNumberGenerator().Randf() < Threshold) continue;
                FillCell(new Vector2(x, y));
            }
        }
    }
    
    private void FillCell(Vector2 position)
    {
        Sprite2D sprite = new();
        sprite.Texture = Database.Instance.GetTexture("Bush");
        sprite.Position = GlobalPosition + position + new Vector2(GD.RandRange(-32, 32), GD.RandRange(-32, 32));
        sprite.Centered = true;
        sprite.Offset = Vector2.Up * sprite.Texture.GetHeight() / 2;
        sprite.Scale = new Vector2(0.25f * new RandomNumberGenerator().RandfRange(0.9f, 1.1f),
            0.25f * new RandomNumberGenerator().RandfRange(0.9f, 1.1f));
        // sprite.FlipH = new RandomNumberGenerator().Randf() > 0.5f;
        sprite.YSortEnabled = true;
        sprite.ZIndex = 1;
        // sprite.RotationDegrees = new RandomNumberGenerator().RandfRange(-5, 5);
        Globals.FactoryScene.GroundItemHolder.AddChild(sprite);
        // AddChild(sprite);
    }
}