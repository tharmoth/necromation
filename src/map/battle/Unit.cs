using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.character;
using Necromation.map;
using Necromation.map.character;
using Necromation.sk;

public partial class Unit : Sprite2D, LayerTileMap.IEntity
{
	
	public Vector2I TargetPosition = Vector2I.Zero;
	private int _hp = 100;
	private double _timestep = 1;
	private double _time = GD.RandRange(0, 1.0);
	private Tween _jiggleTween;
	private Tween _moveTween;
	private Tween _damageTween;
	private string _unitName = MapUtils.GetRandomCommanderName();
	private string _unitType;
	public string Team = "Player";
	private readonly Commander _commander;
	
	protected Sprite2D Sprite = new();
	
	private static Texture2D _texture = new Database().GetTexture("warrior");

	public Unit(string unitType, Commander commander = null)
	{
		_commander = commander;
		_unitType = unitType;
		var texture = _texture;
		Sprite.Texture = texture;
		Sprite.Scale = new Vector2(.5f, .5f);
		AddChild(Sprite);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Sprite.FlipH = Team == "Player";
		AddToGroup("Units");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Every 0.5 seconds, move the unit one cell forward
		_time += delta;
		if (_time < _timestep || _hp <= 0) return;
		_time = 0;

		TargetClosestEnemy();
		if (Attack()) return;

		// If surrounded by 4 units, don't call the expensive pathfinding algorithm
		if (GetAdjacent().Values.Count(unit => unit != null) == 4) return;
		MoveToTarget();
	}
	
	/**************************************************************************
	 * Public Methods                                                         *
	 **************************************************************************/
	public void Damage(int damage)
	{
		_damageTween?.Kill();
		_damageTween = CreateTween();
		_damageTween.TweenProperty(this, "modulate", new Color(1, 0, 0), _timestep / 10);
		_damageTween.TweenProperty(this, "modulate", Colors.White, _timestep / 10);
		
		_hp -= damage;
		if (_hp > 0) return;
		Globals.BattleScene.TileMap.RemoveEntity(this);
		_commander?.Remove(_unitType);
		QueueFree();
	}
	
	/**************************************************************************
	 * Private Methods                                                        *
	 **************************************************************************/
	protected void Jiggle()
	{
		// jiggle the unit
		if (_jiggleTween != null) return;
		_jiggleTween = CreateTween();
		_jiggleTween.SetTrans(Tween.TransitionType.Quad);
		_jiggleTween.SetEase(Tween.EaseType.Out);
		_jiggleTween.TweenProperty(this, "rotation_degrees", 10, _timestep / 10);
		_jiggleTween.TweenProperty(this, "rotation_degrees", 0, _timestep / 10);
		_jiggleTween.TweenCallback(Callable.From(() => _jiggleTween = null));
	}
	
	private void TargetClosestEnemy()
	{
		var closest = GetTree().GetNodesInGroup("Units")
			.Where(unit => unit is Unit && unit != this)
			.Select(unit => unit as Unit)
			.Where(unit => unit.Team != Team)
			.MinBy(unit => unit.GlobalPosition.DistanceSquaredTo(GlobalPosition));
		TargetPosition = Globals.BattleScene.TileMap.GlobalToMap(closest?.GlobalPosition ?? GlobalPosition);
	}
	
	protected virtual bool Attack()
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
		var position = Globals.BattleScene.TileMap.GetEntityPositions(this).First();
		if (position == TargetPosition) return;
		
		var nextPosition = Globals.BattleScene.TileMap.GetNextPath(position, TargetPosition);

		if (!Globals.BattleScene.TileMap.IsEmpty(nextPosition)) return;
		Globals.BattleScene.TileMap.RemoveEntity(this);
		Globals.BattleScene.TileMap.AddEntity(nextPosition, this, BattleTileMap.Unit);

		_moveTween?.Kill();
		_moveTween = CreateTween();
		_moveTween.TweenProperty(this, "global_position", Globals.BattleScene.TileMap.MapToGlobal(nextPosition), _timestep / 10);
	}

	private Unit GetUnitInDirection(Vector2I direction)
	{
		var mapPos = Globals.BattleScene.TileMap.GetEntityPositions(this).First();
		var entity = Globals.BattleScene.TileMap.GetEntities(mapPos + direction, BattleTileMap.Unit);

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
