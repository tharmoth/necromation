using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation;

public class WanderState : IState
{
    public required Actor Actor { private get; init; }

    private const float FactoryExclusionZone = 100.0f;
    private const float VisionRange = 250.0f;
    private const float WanderTime = 2.0f;

    public List<Command> Commands => [new MoveCommand { TargetPosition = _wanderTarget }];
    public IState NextState
    {
        get
        {
            var player = Globals.MapScene.Actors.First(a => a.GetComponent<StatsComponent>().Team == "Player");
            if (!IsPlayerNearActor(Actor, player))
            {
                return this;
            }
            return new AttackState() { Actor = Actor };
        }
    }

    private Vector2 _wanderTarget = Vector2.Inf;
    private float _time;
    
    public void _Process(double delta)
    {
        _time += (float) delta;
        if (_wanderTarget != Vector2.Inf
            && !(_time >= WanderTime)
            ) return;
        _wanderTarget = Actor.GlobalPosition + Utils.RandomPointInCircle(256);
        _time = 0;
    }

    
    private bool IsPlayerNearActor(Actor actor, Actor targetActor)
    {
        return actor.GlobalPosition.DistanceTo(targetActor.GlobalPosition) < VisionRange;
    }
}