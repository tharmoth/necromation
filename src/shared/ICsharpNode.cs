using Godot;

namespace Necromation;

public class CsharpNode
{
    public virtual void _Process(double delta) { }
    public virtual void  _Ready() { }
    public virtual void  _EnterTree() { }
    public virtual void  _ExitTree() { }
    public virtual void  _PhysicsProcess(double delta) { }
}