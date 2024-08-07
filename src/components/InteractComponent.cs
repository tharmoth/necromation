using System;
using System.Collections.Generic;
using Godot;

public interface IMapInteractable
{
    public Vector2 GlobalPosition { get; }
    
    public void Interact(Actor actor);
}

public class InteractComponent : IMapInteractable
{
    public static List<IMapInteractable> MouseOverQueue = [];
    
    public required Node2D Parent { get; init; }
    public Vector2 GlobalPosition => Parent.GlobalPosition;
    public required Action OnInteract { get; init; }
    public void Interact(Actor actor)
    {
        OnInteract();
    }
}