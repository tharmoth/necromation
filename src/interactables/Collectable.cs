using Godot;

namespace Necromation;

public partial class Collectable : Interactable, BuildingTileMap.IBuilding, BuildingTileMap.IEntity
{
    [Export] public string Type { get; set; } = "Stone";
    
    public override void _Ready()
    {
        base._Ready();
        AddToGroup("resources");
        GetNode<Sprite2D>("Sprite2D").Texture = GD.Load<Texture2D>($"res://res/sprites/{Type}.png");
    }
    
    protected override void Complete()
    {
        base.Complete();
        Globals.PlayerInventory.Insert(Type);
    }

    public string ItemType => Type;
    public bool CanRemove()
    {
        return false;
    }
}