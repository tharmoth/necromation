using System.Collections.Generic;

public class NullInputHandler : IInputHandler
{
    public List<Command> HandleInput(Actor actor, double delta)
    {
        return [];
    }
}