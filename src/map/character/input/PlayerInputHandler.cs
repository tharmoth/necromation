
using System.Collections.Generic;
using System.Linq;
using Godot;
using Necromation.map.character;

public class PlayerInputHandler : IInputHandler
{
    private Vector2 _targetPosition;
    private IMapInteractable _targetInteractable;
    
    public PlayerInputHandler(Vector2 startPosition)
    {
        _targetPosition = startPosition;
    }
    
    public List<Command> HandleInput(Actor actor, double delta)
    {
        if (Input.IsActionJustPressed("left_click"))
        {
            if (InteractComponent.MouseOverQueue.Count > 0)
            {
                _targetInteractable = InteractComponent.MouseOverQueue.Last();
                _targetPosition = _targetInteractable.GlobalPosition;
            }
            else
            {
                _targetInteractable = null;
                _targetPosition = actor.GetGlobalMousePosition();
            }
        }
        
        var commands = new List<Command>();
        commands.Add(new MoveCommand { TargetPosition = _targetPosition });

        if (ShouldInteract(actor))
        {
            commands.Add(new InteractCommand { Interactable = _targetInteractable });
            _targetInteractable = null;
        }
        
        return commands;
    }
    

    
    private bool ShouldInteract(Actor actor)
    {
        if (_targetInteractable == null) return false;
        var interactDistance = actor.GetComponent<StatsComponent>().InteractDistance;
        return _targetPosition.DistanceTo(actor.GlobalPosition) < interactDistance;
    }
}