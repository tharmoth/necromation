using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.character;
using Necromation.map;
using Necromation.sk;

public partial class Unit : Sprite2D, LayerTileMap.IEntity
{
	
	public Vector2I TargetPosition = new Vector2I(0, 0);
	private int _hp = 100;
	private double _timestep = 1.0;
	private double _time = GD.RandRange(0, 1.0);
	private Tween _jiggleTween;
	private Tween _moveTween;
	private Tween _damageTween;
	private string _unitName = MapUtils.GetRandomCommanderName();
	public string Team = "Player";
	
	private Sprite2D _sprite = new();

	public Unit()
	{
		var texture = new Database().GetTexture("warrior");
		_sprite.Texture = texture;
		_sprite.Scale = new Vector2(.5f, .5f);
		AddChild(_sprite);
	}


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_sprite.FlipH = Team == "Player";
		BattleGlobals.TempUnit ??= this;
		BattleGlobals.TileMap.AddEntity(GlobalPosition, this, BattleTileMap.Unit);
		GD.Print(_unitName);
		AddToGroup("Units");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Every 0.5 seconds, move the unit one cell forward
		_time += delta;
		if (_time < _timestep) return;
		_time = 0;

		TargetClosestEnemy();
		if (AttackAdjacent()) return;
		MoveToTarget();
	}
	
	/**************************************************************************
	 * Public Methods                                                         *
	 **************************************************************************/
	public void Damage(int damage)
	{
		_damageTween?.Kill();
		_damageTween = CreateTween();
		_damageTween.TweenProperty(this, "modulate", new Color(1, 0, 0), 0.1f);
		_damageTween.TweenProperty(this, "modulate", Colors.White, 0.1f);
		
		_hp -= damage;
		if (_hp > 0) return;
		BattleGlobals.TileMap.RemoveEntity(this);
		QueueFree();
	}
	
	/**************************************************************************
	 * Private Methods                                                        *
	 **************************************************************************/
	private void Jiggle()
	{
		// jiggle the unit
		if (_jiggleTween != null) return;
		_jiggleTween = CreateTween();
		_jiggleTween.SetTrans(Tween.TransitionType.Quad);
		_jiggleTween.SetEase(Tween.EaseType.Out);
		_jiggleTween.TweenProperty(this, "rotation_degrees", 10, 0.1f);
		_jiggleTween.TweenProperty(this, "rotation_degrees", 0, 0.1f);
		_jiggleTween.TweenCallback(Callable.From(() => _jiggleTween = null));
	}
	
	private void TargetClosestEnemy()
	{
		var closest = GetTree().GetNodesInGroup("Units")
			.Where(unit => unit is Unit && unit != this)
			.Select(unit => unit as Unit)
			.Where(unit => unit.Team != Team)
			.MinBy(unit => unit.GlobalPosition.DistanceTo(GlobalPosition));
		TargetPosition = BattleGlobals.TileMap.GlobalToMap(closest?.GlobalPosition ?? GlobalPosition);
	}
	
	private bool AttackAdjacent()
	{
		var adjacent = GetAdjacent();

		foreach (var unit in adjacent.Values.Where(unit => unit != null).Where(unit => unit.Team != Team))
		{
			Jiggle();
			if (GD.Randf() > 0.5) unit.Damage(10);
			return true;
		}

		return false;
	}
	
	private void MoveToTarget()
	{
		var position = BattleGlobals.TileMap.GetEntityPositions(this).First();
		if (position == TargetPosition) return;
		
		var nextPosition = BattleGlobals.TileMap.GetNextPath(position, TargetPosition);

		if (!BattleGlobals.TileMap.IsEmpty(nextPosition)) return;
		BattleGlobals.TileMap.RemoveEntity(this);
		BattleGlobals.TileMap.AddEntity(nextPosition, this, BattleTileMap.Unit);

		_moveTween?.Kill();
		_moveTween = CreateTween();
		_moveTween.TweenProperty(this, "global_position", BattleGlobals.TileMap.MapToGlobal(nextPosition), 0.5f);
	}

	private Unit GetUnitInDirection(Vector2I direction)
	{
		var mapPos = BattleGlobals.TileMap.GetEntityPositions(this).First();
		var entity = BattleGlobals.TileMap.GetEntities(mapPos + direction, BattleTileMap.Unit);

		return entity as Unit;
	}
    
	/// <summary>
	/// Returns a dictionary of adjacent unit. If no belt is found in a direction, the corresponding dictionary value
	/// will be null.
	/// </summary>
	/// <returns>A dictionary with keys {"Up", "Down", "Right", "Left"} and values as the corresponding
	/// adjacent <see cref="Unit"/> instances or null if no unit is present in the direction.</returns>
	private Dictionary<string, Unit> GetAdjacent()
	{
		var up = GetUnitInDirection(SKTileMap.GetUp());
		var down = GetUnitInDirection(SKTileMap.GetDown());
		var left = GetUnitInDirection(SKTileMap.GetLeft());
		var right = GetUnitInDirection(SKTileMap.GetRight());

		return new Dictionary<string, Unit>
		{
			{ "Up", up },
			{ "Down", down },
			{ "Left", right },
			{ "Right", left }
		};
	}
}
