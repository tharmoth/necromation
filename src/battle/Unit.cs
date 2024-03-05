using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.map;
using Necromation.map.battle.Weapons;
using Necromation.map.character;
using Necromation.sk;

public partial class Unit : Sprite2D, LayerTileMap.IEntity
{
	public Vector2I MapPosition => Globals.BattleScene.TileMap.GlobalToMap(GlobalPosition);
	public Vector2I TargetPosition = Vector2I.Zero;
	public Vector2 CachedPosition;
	
	private readonly Commander _commander;
	public string Team = "Player";
	public string UnitType;
	public readonly  List<Weapon> Weapons = new();
	public readonly List<Armor> Armor = new();
	
	private int _hp = 10;

	public double Cooldown = GD.RandRange(0, BattleScene.TimeStep);
	private Tween _jiggleTween;
	private Tween _bobTween;
	private Tween _moveTween;
	private Tween _damageTween;

	protected Sprite2D Sprite = new();
	private List<Unit> enemies = null;
	public AudioStreamPlayer2D Audio = new AudioStreamPlayer2D();
	private Unit _target;
	public int Ammo = 12;
	public int Strength = 10;
	
	public readonly List<Action<Unit>> DeathCallbacks = new();
	

	public Unit(string unitType, Commander commander = null)
	{
		UnitType = unitType;
		_commander = commander;

		var def = Database.Instance.Units.First(unit => unit.Name == unitType);
		foreach (var weapon in def.Weapons)
		{
			Weapons.Add(Database.Instance.Equpment.OfType<Weapon>().First(weaponData => weaponData.Name == weapon));
		}
		Weapons.Add(new MeleeWeapon("Fist", 1, 1, 1, 1));
		
		foreach (var armor in def.Armor)
		{
			Armor.Add(Database.Instance.Equpment.OfType<Armor>().First(armorDef => armorDef.Name == armor));
		}

		Sprite.Texture = Database.Instance.GetTexture(UnitType, "unit");
		Sprite.Scale = new Vector2(.125f, .125f);
		// Sprite.Scale = new Vector2(.5f, .5f);
		AddChild(Sprite); 
		AddChild(Audio);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Sprite.FlipH = Team != "Player";
		// Sprite.Modulate = Team == "Player" ? new Color(.8f, .8f, 1) : new Color(1, .8f, .8f);
		AddToGroup(Team+"Units");
		CachedPosition = GlobalPosition;
	}

	private void initEnemies()
	{
		enemies ??= GetTree()
			.GetNodesInGroup(Team == "Player" ? "EnemyUnits" : "PlayerUnits")
			.OfType<Unit>()
			.ToList();
		enemies.ForEach(unit =>
		{
			unit.DeathCallbacks.Add((_) => enemies.Remove(unit));
		});
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (enemies == null) initEnemies();
		
		// Every 0.5 seconds, move the unit one cell forward
		Cooldown += delta;
		if (Cooldown < BattleScene.TimeStep || _hp <= 0) return;
		Cooldown = 0;

		foreach (var weapon in Weapons.Where(weapon => weapon.CanAttackDeclump(this, enemies)))
		{
			weapon.Attack(this);
			return;
		}

		if (!IsInstanceValid(_target))
		{
			var rand = GD.Randf();
			if (rand < 0.25)
			{
				_target = TargetRandomEnemy();
			}
			else if (rand < 0.5)
			{
				_target = TargetClosestEnemy();
			}
		}

		UpdateTargetPosition();

		MoveToTarget();
		
		foreach (var weapon in Weapons.Where(weapon => weapon.CanAttack(this, enemies)))
		{
			weapon.Attack(this);
			return;
		}
	}
	
	/**************************************************************************
	 * Public Methods                                                         *
	 **************************************************************************/
	public void Damage(Unit source, int damage)
	{
		_damageTween?.Kill();
		_damageTween = CreateTween();
		_damageTween.TweenProperty(this, "modulate", new Color(1, 0, 0), BattleScene.TimeStep / 5);
		_damageTween.TweenProperty(this, "modulate", Colors.White, BattleScene.TimeStep /  5);

		RichTextLabel text = new();
		Globals.BattleScene.AddChild(text);
		text.Text = "[color=red]" + damage.ToString() + "[/color]";
		text.GlobalPosition = GlobalPosition;
		text.CustomMinimumSize = new Vector2(100, 100);
		text.BbcodeEnabled = true;

		var labelTween = GetTree().CreateTween();
		labelTween.TweenProperty(text, "global_position", text.GlobalPosition + new Vector2(0, -50), 1.0f);
		labelTween.TweenCallback(Callable.From(() =>
		{
			if (!IsInstanceValid(text)) return;
			text.QueueFree();
		}));
		
		var textColorTween = GetTree().CreateTween();
		textColorTween.TweenProperty(text, "modulate", new Color(1, 1, 1, 0), 1.0f);
        
		_hp -= damage;
		if (_hp > 0) return;

		PlayDeathAnimation();
		PlayDeathSound();
		
		DeathCallbacks.ForEach(callback => callback(source));

		Globals.BattleScene.TileMap.RemoveEntity(this);
		_commander?.Remove(UnitType);
		QueueFree();
	}
	
	public void Jiggle()
	{
		// jiggle the unit
		if (_jiggleTween != null) return;
		_jiggleTween = CreateTween();
		_jiggleTween.SetTrans(Tween.TransitionType.Quad);
		_jiggleTween.SetEase(Tween.EaseType.Out);
		_jiggleTween.TweenProperty(this, "rotation_degrees", 10, BattleScene.TimeStep /  5);
		_jiggleTween.TweenProperty(this, "rotation_degrees", 0, BattleScene.TimeStep /  5);
		_jiggleTween.TweenCallback(Callable.From(() => _jiggleTween = null));
	}
	/**************************************************************************
	 * Private Methods                                                        *
	 **************************************************************************/
	private Unit TargetClosestEnemy()
	{
		var closest = enemies.MinBy(unit => unit.GlobalPosition.DistanceSquaredTo(GlobalPosition));
		TargetPosition = Globals.BattleScene.TileMap.GlobalToMap(closest?.GlobalPosition ?? GlobalPosition);
		return closest;
	}

	private Unit TargetRandomEnemy()
	{
		if (enemies.Count == 0)
		{
			TargetPosition = MapPosition;
			return null;
		}
		var closest  = enemies.ElementAt(GD.RandRange(0, enemies.Count - 1));
		TargetPosition = closest?.MapPosition ?? MapPosition;
		return closest;
	}

	private void UpdateTargetPosition()
	{
		TargetPosition = !IsInstanceValid(_target) ? MapPosition : _target.MapPosition;
	}

	private void MoveToTarget()
	{
		if (MapPosition == TargetPosition) return;
		if (Globals.BattleScene.TileMap.IsSurrounded(MapPosition)) return;
		
		// var nextPosition = Globals.BattleScene.TileMap.GetNextPath(MapPosition, TargetPosition);
		var nextPosition = getNextPosition();
		
		// nextPosition 
		// nextPosition = Globals.BattleScene.TileMap.GetNextPath(MapPosition, TargetPosition);


		if (nextPosition == MapPosition) return;

		if (!Globals.BattleScene.TileMap.IsEmpty(nextPosition)) return;
		Globals.BattleScene.TileMap.RemoveEntity(this);
		Globals.BattleScene.TileMap.AddEntity(nextPosition, this, BattleTileMap.Unit);

		var nextPositionGlobal = Globals.BattleScene.TileMap.MapToGlobal(nextPosition) + new Vector2((float)GD.RandRange(-5.0, 5.0), (float)GD.RandRange(-5.0, 5.0));
		
		_moveTween?.Kill();
		_moveTween = CreateTween();
		_moveTween.TweenProperty(this, "global_position", nextPositionGlobal, BattleScene.TimeStep);

		_bobTween?.Kill();
		_bobTween = CreateTween();
		_bobTween.TweenProperty(Sprite, "position", new Vector2(0, -5), BattleScene.TimeStep / 2);
		_bobTween.TweenProperty(Sprite, "position", new Vector2(0, 0), BattleScene.TimeStep / 2);
		CachedPosition = Globals.BattleScene.TileMap.MapToGlobal(nextPosition);
	}

	private Vector2I getNextPosition()
	{
		var differance = TargetPosition - MapPosition;

		var directions = new List<(Vector2I, bool)>
		{
			(new Vector2I(1, 0), differance.X >= 0),  // Right
			(new Vector2I(-1, 0), differance.X <= 0), // Left
			(new Vector2I(0, 1), differance.Y >= 0),  // Down
			(new Vector2I(0, -1), differance.Y <= 0)  // Up
		};
		
		// Try to move in the direction of the target that has the highest differnece.
		// If the way forward is blocked try moving in the other direction.
		if (Mathf.Abs(differance.X) > Mathf.Abs(differance.Y) || Mathf.Abs(differance.X) == Mathf.Abs(differance.Y))
		{
			if (directions[0].Item2 && IsEmpty(MapPosition + directions[0].Item1))
			{
				return MapPosition + directions[0].Item1;
			} 
			else if (directions[1].Item2 && IsEmpty(MapPosition + directions[1].Item1))
			{
				return MapPosition + directions[1].Item1;
			}
			else if (directions[2].Item2 && IsEmpty(MapPosition + directions[2].Item1))
			{
				return MapPosition + directions[2].Item1;
			}
			else if (directions[3].Item2 && IsEmpty(MapPosition + directions[3].Item1))
			{
				return MapPosition + directions[3].Item1;
			} else if (IsEmpty(MapPosition + directions[2].Item1))
			{
				return MapPosition + directions[2].Item1;
			} 
			else if (IsEmpty(MapPosition + directions[3].Item1))
			{
				return MapPosition + directions[3].Item1;
			}
		}
		else
		{
			if (directions[2].Item2 && IsEmpty(MapPosition + directions[2].Item1))
			{
				return MapPosition + directions[2].Item1;
			}
			else if (directions[3].Item2 && IsEmpty(MapPosition + directions[3].Item1))
			{
				return MapPosition + directions[3].Item1;
			}
			else if (directions[0].Item2 && IsEmpty(MapPosition + directions[0].Item1))
			{
				return MapPosition + directions[0].Item1;
			} 
			else if (directions[1].Item2 && IsEmpty(MapPosition + directions[1].Item1))
			{
				return MapPosition + directions[1].Item1;
			}
			else if (IsEmpty(MapPosition + directions[0].Item1))
			{
				return MapPosition + directions[0].Item1;
			} 
			else if (IsEmpty(MapPosition + directions[1].Item1))
			{
				return MapPosition + directions[1].Item1;
			}
		}

		return MapPosition;
	}

	private bool IsEmpty(Vector2I mapPos)
	{
		return Globals.BattleScene.TileMap.IsEmpty(mapPos) && Globals.BattleScene.TileMap.IsOnMap(mapPos);
	}
	
		
	private void PlayDeathAnimation()
	{
		RemoveChild(Sprite);
		Globals.BattleScene.AddChild(Sprite);

		var randomPos = new Vector2(GD.RandRange(-16, 16), GD.RandRange(8, 16));
		var target = GlobalPosition + randomPos;

		float targetDegrees;
		if (Team == "Player")
		{
			targetDegrees = randomPos.X > 0 ? -90 + GD.RandRange(-10, 10) : 90 + GD.RandRange(-10, 10);
		}
		else
		{
			targetDegrees = randomPos.X < 0 ? -90 + GD.RandRange(-10, 10) : 90 + GD.RandRange(-10, 10);
		}

		
		Sprite.GlobalPosition = GlobalPosition;
		Sprite.Modulate = new Color(.5f, .5f, .5f);
		Sprite.ZIndex = -1;

		var deathTween = GetTree().CreateTween();
		deathTween.SetTrans(Tween.TransitionType.Sine);
		deathTween.TweenProperty(Sprite, "global_position", target, BattleScene.TimeStep);
		
		var deathRotationTween = GetTree().CreateTween();
		deathRotationTween.SetTrans(Tween.TransitionType.Quad);
		deathRotationTween.SetEase(Tween.EaseType.In);
		deathRotationTween.TweenProperty(Sprite, "rotation_degrees", targetDegrees, BattleScene.TimeStep);
	}

	private void PlayDeathSound()
	{
		var randomizer = new AudioStreamRandomizer();
		randomizer.AddStream(0, Database.LoadAudioFileFromFolder("death"));

		var audio = new AudioStreamPlayer2D();
		audio.Stream = randomizer;
		audio.VolumeDb = -40;
		Sprite.AddChild(audio);
		audio.Play();
	}
}
