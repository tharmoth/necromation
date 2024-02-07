using Godot;

namespace Necromation;

public partial class Collectable : Interactable
{
    [Export]
    private string type;
    
    public override void _Ready()
    {
        base._Ready();
        AddToGroup("resources");
    }
    
    protected override void Complete()
    {
        base.Complete();
        Inventory.Instance.AddItem(type);
        QueueFree();
    }
}