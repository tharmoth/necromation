using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation;

public abstract class Command
{
    public abstract void Execute(Actor actor, double delta);
}

public interface IInputHandler
{
    List<Command> HandleInput(Actor actor, double delta);
}

public partial class Actor : Node2D
{
    /**************************************************************************
     * Private Variables                                                      *
     **************************************************************************/
    private readonly List<object> _components = [];
    private readonly IInputHandler _inputHandler;

    /**************************************************************************
     * Godot Methods                                                          *
     **************************************************************************/
    public override void _Process(double delta)
    {
        var commands = _inputHandler.HandleInput(this, delta);
        foreach (var command in commands)
        {
            command.Execute(this, delta);
        }

        foreach (var component in _components.OfType<Component>())
        {
            component._Process(delta);
        }
    }
    
    /**************************************************************************
     * Public Methods                                                         *
     **************************************************************************/
    public void AddComponent(object component)
    {
        _components.Add(component);
        if (component is Node node)
        {
            AddChild(node);
        }
    }
    
    public void RemoveComponent(object component)
    {
        _components.Remove(component);
        if (component is Node node && node.GetParent() == this)
        {
            RemoveChild(node);
        }
    }
	
    public T GetComponent<T>() where T : class
    {
        return _components.Find(c => c is T) as T;
    }
	
    public List<T> GetComponents<T>() where T : class
    {
        return _components.OfType<T>().ToList();
    }
}