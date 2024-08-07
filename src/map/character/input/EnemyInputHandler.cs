using System.Collections.Generic;

public class EnemyInputHandler : IInputHandler
{
    private IState _currentState;
    public List<Command> HandleInput(Actor actor, double delta)
    {
        if (IsDead(actor))
        {
            actor.QueueFree();
            return [];
        }

        // If the player is near the factory wander
        // If the player is farther away then X wander
        // Otherwise attack the player
        _currentState ??= new WanderState() { Actor = actor };
        _currentState = _currentState.NextState;
        _currentState._Process(delta);
        var commands = _currentState.Commands;
        
        return commands;
    }

    private bool IsDead(Actor actor)
    {
        return actor.GetComponent<ArmyComponent>().Commanders.Count <= 0;
    }
}