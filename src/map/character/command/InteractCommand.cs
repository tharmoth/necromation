
public class InteractCommand : Command
{
    public required IMapInteractable Interactable { get; init; }
    
    public override void Execute(Actor actor, double delta)
    {
        Interactable.Interact(actor);
    }
}