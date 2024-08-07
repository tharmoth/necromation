using Godot;

public class StopCommand : Command
{
    public override void Execute(Actor actor, double delta)
    {
        actor.GetComponent<VelocityComponent>().Velocity = Vector2.Zero;
    }
}