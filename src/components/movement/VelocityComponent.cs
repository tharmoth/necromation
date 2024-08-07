using Godot;
using Necromation.components;

public class VelocityComponent : Component
{
    public Vector2 Velocity { get; set; }
    public required Node2D Parent { get; init; }

    public override void _Process(double delta)
    {
        Parent.Position += Velocity * (float) delta;
    }
}