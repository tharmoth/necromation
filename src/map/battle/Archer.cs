using Godot;
using Necromation.map.character;

namespace Necromation.map.battle;

public partial class Archer : Unit
{
    public Archer(Commander commander = null) : base("Archer", commander)
    {
        Sprite.Texture = GD.Load<Texture2D>("res://res/sprites/archer.png");
    }
    
    protected override bool Attack()
    {
        if (TargetPosition == Vector2I.Zero) return false;
        
        var arrow = new Arrow(TargetPosition);
        arrow.GlobalPosition = GlobalPosition;
        GetParent().AddChild(arrow);
        Jiggle();
        return true;
    }
}