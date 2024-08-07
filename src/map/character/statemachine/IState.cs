using System.Collections.Generic;

public interface IState
{
    public List<Command> Commands { get; }
    public IState NextState { get; }
    public void _Process(double delta);
}