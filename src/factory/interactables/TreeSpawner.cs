using Godot;
using Necromation.sk;

namespace Necromation;

public partial class TreeSpawner : Node2D
{
    public int Radius = 1000;
    public int NoiseSeed;
    public float Threshold = 0.5f;
    
    public override void _Ready()
    {
        base._Ready();
        YSortEnabled = true;
        CallDeferred("Spawn");
    }
    
    public void Spawn()
    {
        var smallNoise = new FastNoiseLite();
        smallNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        smallNoise.Seed = NoiseSeed;
        smallNoise.Frequency = 0.005f;
        smallNoise.Offset = new Vector3(GlobalPosition.X, GlobalPosition.Y, 0);
		
        var bigNoise = new FastNoiseLite();
        bigNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        bigNoise.Seed = NoiseSeed;
        bigNoise.Frequency = 0.0005f;
        bigNoise.Offset = new Vector3(GlobalPosition.X, GlobalPosition.Y, 0);

        for (var x = -Radius; x < Radius; x += 64)
        {
            for (var y = -Radius; y < Radius; y += 64)
            {
                var position = new Vector2(x, y);
                // if ((Utils.NoiseNorm(smallNoise, position) + Utils.NoiseNorm(bigNoise, position)) / 2.0 < Threshold) continue;
                if (Utils.NoiseNorm(bigNoise, position) < Threshold) continue;
                FillCell(new Vector2(x, y));
            }
        }
    }

    private void FillCell(Vector2 position)
    {
        Sprite2D sprite = new();
        sprite.Texture = Database.Instance.GetTexture("Tree");
        sprite.Position = GlobalPosition + position + new Vector2(GD.RandRange(-32, 32), GD.RandRange(-32, 32));
        sprite.Centered = true;
        sprite.Offset = Vector2.Up * sprite.Texture.GetHeight() / 2;
        sprite.Scale = new Vector2(0.25f * new RandomNumberGenerator().RandfRange(0.8f, 1.2f),
                0.25f * new RandomNumberGenerator().RandfRange(0.9f, 1.1f));
        sprite.FlipH = new RandomNumberGenerator().Randf() > 0.5f;
        sprite.YSortEnabled = true;
        sprite.ZIndex = 1;
        sprite.RotationDegrees = new RandomNumberGenerator().RandfRange(-5, 5);
        Globals.FactoryScene.GroundItemHolder.AddChild(sprite);
        // AddChild(sprite);
    }
}