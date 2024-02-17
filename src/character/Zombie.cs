using Godot;
using System.Linq;
using Necromation;

public partial class Zombie : Node2D
{
	private float _speed = 25;
	private Mode _mode = Mode.Follow;
	
	private Node2D _target;

	public enum Mode
	{
		Search, Return, Follow, Interacting
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		switch (_mode)
		{
			case Mode.Search:
				_search(delta);
				break;
			case Mode.Interacting:
				_interact(delta);
				break;
			case Mode.Return:
				_return(delta);
				break;
            case Mode.Follow:
                _follow(delta);
                break;
		}
	}
	
	private void _moveToTarget(double delta)
	{
		if (_target == null) return;
		var direction = (_target.GlobalPosition - GlobalPosition).Normalized();
		GlobalPosition += direction * _speed * (float)delta;
	}

	private void _search(double delta)
	{
		if (!IsInstanceValid(_target)) _target = null;
		if (_target is Interactable interact && interact.Interacting()) _target = null;
		
		_target ??= GetTree().GetNodesInGroup("resources")
			.ToList()
			.Select(x => (Interactable)x)
			.Where(x => x.CanInteract())
			.MinBy(x => GlobalPosition.DistanceTo(x.GlobalPosition));

		_moveToTarget(delta);

		if (!IsInstanceValid(_target) || GlobalPosition.DistanceTo(_target.GlobalPosition) > 10 || _target is not Interactable interactable) return;

		interactable.Interact();
		_mode = Mode.Interacting;
	}
	
	private void _interact(double delta)
	{
		if (!IsInstanceValid(_target))
		{
			_target = null;
			_mode = Mode.Return;
		}
		if (_target is Interactable interactable && interactable.CanInteract())
		{
			interactable.Interact();
		}
	}

	private void _return(double delta)
	{
		_target ??= GetTree().GetNodesInGroup("alters")
			.ToList()
			.Select(x => (Interactable)x)
			.MinBy(x => GlobalPosition.DistanceTo(x.GlobalPosition));

		if (_target == null) return;

		_moveToTarget(delta);

		if (GlobalPosition.DistanceTo(_target.GlobalPosition) < 10)
		{ 
			_mode = Mode.Search;
			_target = null;
		}
	}
    
    private void _follow(double delta) 
    {
	    _target ??= GetTree().GetNodesInGroup("player")
		    .ToList()
		    .Select(x => (Character)x)
		    .FirstOrDefault();

        if (_target == null || GlobalPosition.DistanceTo(_target.GlobalPosition) < 50) return;
        
        _moveToTarget(delta);
    }
    
    public override void _Input(InputEvent @event)
    {
	    var sprite = GetNode<Sprite2D>("Sprite2D");
	    if (@event is not InputEventMouseButton eventMouseButton) return;
	    if (!eventMouseButton.Pressed || eventMouseButton.ButtonIndex != MouseButton.Left) return;
	    if (!sprite.GetRect().HasPoint(sprite.ToLocal(GetGlobalMousePosition()))) return;

	    // Inventory.Instance.SelectedZombie = this;
    }
    
    public void SetTarget(Node2D target)
    {
	    _target = target;
    }
    
    public void SetMode(Mode mode)
	{
	    _mode = mode;
	}
}
