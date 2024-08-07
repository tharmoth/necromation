using Godot;

public class MoveCommand : Command
{
    public required Vector2 TargetPosition { private get; init; }
    
    public override void Execute(Actor actor, double delta)
    {
        var stats = actor.GetComponent<StatsComponent>();
        var velocity = actor.GetComponent<VelocityComponent>();
        var direction = TargetPosition - actor.GlobalPosition;
        
        if (ShouldMove(actor, TargetPosition, delta))
        {
            velocity.Velocity = direction.Normalized() * stats.WalkSpeed;
        }
        else
        {
            velocity.Velocity = Vector2.Zero;
        }
    }
    
    private static bool ShouldMove(Actor actor, Vector2 target, double delta)
    {
        var distance = target.DistanceTo(actor.GlobalPosition);
        var distanceToMove = actor.GetComponent<StatsComponent>().WalkSpeed * (float)delta;
        return distance > distanceToMove;
    }
}