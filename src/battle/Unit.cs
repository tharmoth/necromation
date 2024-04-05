using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.battle;
using Necromation.battle.Weapons;
using Necromation.map;
using Necromation.map.battle.Weapons;
using Necromation.map.character;
using Necromation.sk;

public class Unit : CsharpNode, LayerTileMap.IEntity
{
	private static readonly PackedScene DeathAudioScene = GD.Load<PackedScene>("res://src/battle/DeathPlayer.tscn");
	private static readonly PackedScene HitAudioScene = GD.Load<PackedScene>("res://src/battle/HitPlayer.tscn");
	
	/**************************************************************************
	 * Position Variables 			     									  *
	 **************************************************************************/
	public Vector2I MapPosition => Globals.BattleScene.TileMap.GlobalToMap(GlobalPosition);
	public Vector2 GlobalPosition;
	private Vector2I _targetPosition = Vector2I.Zero;
	private List<Unit> Enemies => Globals.UnitManager.GetGroup(Team == "Player" ? "Enemy" : "Player");
	
	/**************************************************************************
	 * Logic Variables 			     										  *
	 **************************************************************************/
	public double Cooldown = GD.RandRange(0, BattleScene.TimeStep);
	public readonly  List<Weapon> Weapons = new();
	public readonly List<Armor> Armor = new();
	public readonly Mount Mount = new();
	private double MoveCooldown => _moveCooldownMultiplier * BattleScene.TimeStep;
	private double _moveCooldownMultiplier = 1;
	private readonly Commander _commander;
	public readonly string Team;
	public readonly string UnitType;
	public readonly List<Action<Unit>> DeathCallbacks = new();
	
	private Unit _target;
	
	/**************************************************************************
	 * Visuals Variables 													  *
	 **************************************************************************/
	private readonly Sprite2D BodySprite = new();
	public readonly AudioStreamPlayer2D Audio = new();
	
	public readonly Node2D SpriteHolder = new();
	private Tween _jiggleTween;
	private Tween _bobTween;
	private Tween _moveTween;
	private Tween _damageTween;
	private readonly AudioStreamPlayer2D _deathAudio = DeathAudioScene.Instantiate<AudioStreamPlayer2D>();
	private readonly AudioStreamPlayer2D _hitAudio = HitAudioScene.Instantiate<AudioStreamPlayer2D>();
	
	/**************************************************************************
	 * RPG Data Variables 													  *
	 **************************************************************************/
	private int _hp = 10;
	public readonly int Strength = 10;
	public Dictionary<Weapon, int> Ammo = new();
	public bool IsMoving => _moveTween != null;

	/// <summary>
	/// Constructor loads in the units data and sets up the appropriate RPG data and Visuals.
	/// </summary>
	/// <param name="unitType">String used to load in unit data and update commander on death</param>
	/// <param name="position">Global position that the unit is located at on start</param>
	/// <param name="commander">This units commander, used to update unit counts on death</param>
	public Unit(string unitType, Vector2 position, Commander commander, bool first = false)
	{
		UnitType = unitType;
		_commander = commander;
		GlobalPosition = position;
		Team = commander.Team;

		var def = Database.Instance.Units.First(unit => unit.Name == unitType);
		foreach (var weapon in def.Weapons)
		{
			var weaponData = Database.Instance.Equpment.OfType<Weapon>().First(weaponData => weaponData.Name == weapon);
			Weapons.Add(weaponData);
			Ammo.Add(weaponData, weaponData.Ammo);
		}

		var fist = new MeleeWeapon("Fist", 1, 1, 1, 1, -1);
		Weapons.Add(fist);
		Ammo.Add(fist, fist.Ammo);

		foreach (var armor in def.Armor)
		{
			Armor.Add(Database.Instance.Equpment.OfType<Armor>().First(armorDef => armorDef.Name == armor));
		}

		BodySprite.Texture = Database.Instance.GetTexture(UnitType, "unit");
		BodySprite.Scale = new Vector2(.125f, .125f);
		BodySprite.FlipH = Team != "Player";

		if (def.Mount != "None")
		{
			Mount = new Mount();
			// BodySprite.Modulate = new Color(1, 0, 0);
			_moveCooldownMultiplier = 1 / 3.0;
			if (first)
			{
				BodySprite.Texture = Database.Instance.GetTexture("Bannerhorse", "unit");
			}

		}

		SpriteHolder.AddChild(BodySprite); 
		SpriteHolder.AddChild(Audio);
		Globals.UnitManager.AddToGroup(this, Team);
		
		Globals.BattleScene.TileMap.AddEntity(GlobalPosition, this, BattleTileMap.Unit);

		BodySprite.AddChild(_deathAudio);
		BodySprite.AddChild(_hitAudio);
	}

	public override void _Ready()
	{
		base._Ready();
		SpriteHolder.GlobalPosition = GlobalPosition;
		switch (_commander.CurrentCommand)
		{
			case Commander.Command.Attack:
				break;
			case Commander.Command.HoldAndAttack:
				Cooldown += BattleScene.TimeStep * 12;
				break;
			case Commander.Command.Fire:
				break;
			case Commander.Command.HoldAndFire:
				break;
			case Commander.Command.FireAndKeepDistance:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public override void _Process(double delta)
	{
		if (!Globals.BattleScene.Gui.Started) return;
		
		Cooldown -= delta;
		if (Cooldown >= 0 || _hp <= 0) return;

		// Walk through the units state machine every time the cooldown is over and attack if possible.
		// State Machine Flow
		// 1. Choose a target from the list of desired targets.
		// 2. Attack the desired target if possible (with reduced range).
		// 3. Attack any adjacent enemies
		// 4. Move Towards the targets location
		// 5. Attack the desired target if possible. (with full range).
		
		if (!Globals.UnitManager.IsUnitAlive(_target)) ChooseDesiredTarget();
		else UpdateTargetPosition();
		
		if (AttackDesiredTargetDeclump()) return;
		if (AttackAdjacent()) return;
		if (MoveToTarget()) return;
		if (AttackDesiredTarget()) return;
		
		// wait
		Cooldown += BattleScene.TimeStep;
	}
	
	/**************************************************************************
	 *                          State Machine Methods  					      *
	 *	                                                                      *
	 * We should think about moveing these somewhere else when unit AI is more*
	 * formalized.														      *
	 **************************************************************************/
	/// <summary>
	/// Updates the the position this unit is trying to move to in order to account for target movement.
	/// </summary>
	private void UpdateTargetPosition()
	{
		_targetPosition = !Globals.UnitManager.IsUnitAlive(_target) ? MapPosition : _target.MapPosition;
	}
	
	/// <summary>
	/// Randomly choose a new target or keep the current one. Used to help with unit pathing getting stuck.
	/// </summary>
	private void ChooseDesiredTarget()
	{
		if (Globals.UnitManager.IsUnitAlive(_target)) return;

		switch (_commander.TargetType)
		{
			case Commander.Target.Closest:
				_target = TargetClosestEnemy();
				break;
			case Commander.Target.Archers:
				_target = TargetArcher();
				break;
			case Commander.Target.Cavalry:
				_target = TargetClosestEnemy();
				break;
			case Commander.Target.Rearmost:
				_target = TargetRearmostEnemy();
				break;
			case Commander.Target.Random:
				_target = TargetRandomEnemy();
				break;
			default:
				throw new ArgumentOutOfRangeException();
		};
		
		if (_target == null) _target = TargetRandomEnemy();
	}
	
	/// <summary>
	/// Attempts to move the unit towards the target position and returns true if it was able to move. Otherwise the 
	/// path is blocked and it returns false.
	/// </summary>
	/// <returns>
	/// True if an action was taken.
	/// False if no action was taken.
	/// </returns>
	private bool MoveToTarget()
	{
		if (MapPosition == _targetPosition) return false;
		if (Globals.BattleScene.TileMap.IsSurrounded(MapPosition)) return false;

		var nextPosition = GetNextPosition();
		if (nextPosition == MapPosition) return false;
		if (!Globals.BattleScene.TileMap.IsEmpty(nextPosition)) return false;

		Globals.BattleScene.TileMap.RemoveEntity(MapPosition, BattleTileMap.Unit);
		Globals.BattleScene.TileMap.AddEntity(nextPosition, this, BattleTileMap.Unit);
		var nextPositionGlobal = Globals.BattleScene.TileMap.MapToGlobal(nextPosition) + new Vector2((float)GD.RandRange(-5.0, 5.0), (float)GD.RandRange(-5.0, 5.0));
		
		PlayMovementAnimation(nextPositionGlobal);
		GlobalPosition = Globals.BattleScene.TileMap.MapToGlobal(nextPosition);
		Cooldown = MoveCooldown;
		return true;
	}
	
	/// <summary>
	/// Attempts to make an attack  against the desired target  with any weapon with reduced range to try and declump the units.
	/// </summary>
	/// <returns>
	/// True if an action was taken.
	/// False if no action was taken.
	/// </returns>
	private bool AttackDesiredTargetDeclump()
	{
		if (_target == null) return false;
		foreach (var weapon in Weapons.Where(weapon => weapon.CanAttackDeclump(this, new List<Unit> { _target })))
		{
			weapon.Attack(this);
			return true;
		}		
		return false;
	}
	
	/// <summary>
	/// Attempts to make an attack against the desired target with any weapon.
	/// </summary>
	/// <returns>
	/// True if an action was taken.
	/// False if no action was taken.
	/// </returns>
	private bool AttackDesiredTarget()
	{
		if (_target == null) return false;
		foreach (var weapon in Weapons.Where(weapon => weapon.CanAttack(this, new List<Unit> { _target })))
		{
			weapon.Attack(this);
			return true;
		}		
		return false;
	}
	
	/// <summary>
	/// Attempts to attack any adjacent enemy with any weapon.
	/// </summary>
	/// <returns>
	/// True if an action was taken.
	/// False if no action was taken.
	/// </returns>
	private bool AttackAdjacent()
	{
		var up = Globals.BattleScene.TileMap.GetEntity(MapPosition + Vector2I.Up, BattleTileMap.Unit) as Unit;
		var down = Globals.BattleScene.TileMap.GetEntity(MapPosition + Vector2I.Down, BattleTileMap.Unit) as Unit;
		var left = Globals.BattleScene.TileMap.GetEntity(MapPosition + Vector2I.Left, BattleTileMap.Unit) as Unit;
		var right = Globals.BattleScene.TileMap.GetEntity(MapPosition + Vector2I.Right, BattleTileMap.Unit) as Unit;
		
		var adjacent = new List<Unit> { up, down, left, right };
		var adjacentEnemies = adjacent.Where(unit => unit != null && unit.Team != Team).ToList();
		
		foreach (var weapon in Weapons.Where(weapon => weapon.CanAttack(this, adjacentEnemies, 1)))
		{
			weapon.Attack(this);
			return true;
		}

		return false;
	}
	
	/**************************************************************************
	 * Health Methods                                                         *
	 **************************************************************************/
	public void Damage(Unit source, int damage)
	{
		if (_hp < 0) return;
		
		PlayDamageAnimation();
		ShowText(damage);
		
		_hp -= damage;
		if (_hp > 0) return; 
		OnDeath();
		DeathCallbacks.ForEach(callback => callback(source));
	}

	private void OnDeath()
	{
		PlayDeathAnimation();
		PlayDeathSound();
		
		Globals.BattleScene.TileMap.RemoveEntity(this);
		Globals.UnitManager.RemoveUnit(this);
		_commander?.Remove(UnitType);
	}
	
	/**************************************************************************
	 * Targeting Methods                                                      *
	 **************************************************************************/
	private Unit TargetClosestEnemy()
	{
		var closest = Enemies.MinBy(unit => unit.GlobalPosition.DistanceSquaredTo(GlobalPosition));
		_targetPosition = Globals.BattleScene.TileMap.GlobalToMap(closest?.GlobalPosition ?? GlobalPosition);
		return closest;
	}

	private Unit TargetRandomEnemy()
	{
		if (Enemies.Count == 0)
		{
			_targetPosition = MapPosition;
			return null;
		}
		var closest  = Enemies.ElementAt(GD.RandRange(0, Enemies.Count - 1));
		_targetPosition = closest?.MapPosition ?? MapPosition;
		return closest;
	}
	
	private Unit TargetRearmostEnemy()
	{
		var rearmost = Enemies.MaxBy(unit => unit.GlobalPosition.DistanceSquaredTo(GlobalPosition));
		_targetPosition = Globals.BattleScene.TileMap.GlobalToMap(rearmost?.GlobalPosition ?? GlobalPosition);
		return rearmost;
	}

	private Unit TargetArcher()
	{
		var archers = Enemies.Where(unit => unit.UnitType.Contains("Archer")).ToList();
		if (archers.Count == 0) return TargetRandomEnemy();
		var archer = archers.ElementAt(GD.RandRange(0, archers.Count - 1));
		_targetPosition = Globals.BattleScene.TileMap.GlobalToMap(archer?.GlobalPosition ?? GlobalPosition);
		return archer;
	}

	/// <summary>
	///  Returns the next position the unit should move to. Looks towards the target and tries to move in that direction.
	///  If the way forward is blocked it tries to move in other directions.
	/// </summary>
	/// <returns>An empty target tile or the current position</returns>
	private Vector2I GetNextPosition()
	{
		var differance = _targetPosition - MapPosition;
		differance = new Vector2I(differance.X * 3, differance.Y);

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

	private bool IsEmpty(Vector2I mapPos) => Globals.BattleScene.TileMap.IsEmpty(mapPos) && Globals.BattleScene.TileMap.IsOnMap(mapPos);

	/**************************************************************************
	 * FX Methods                                                             *
	 **************************************************************************/
	/// <summary>
	/// Plays a walking animation that moves the unit sprite to the next position.
	/// </summary>
	/// <param name="nextPositionGlobal">Position to animate movement towards from current position</param>
	private void PlayMovementAnimation(Vector2 nextPositionGlobal)
	{
		BodySprite.FlipH = nextPositionGlobal.X < GlobalPosition.X;
		
		_moveTween?.Kill();
		_moveTween = SpriteHolder.CreateTween();
		_moveTween.TweenProperty(SpriteHolder, "global_position", nextPositionGlobal, MoveCooldown);

		_bobTween?.Kill();
		_bobTween = SpriteHolder.CreateTween();
		_bobTween.TweenProperty(BodySprite, "position", new Vector2(0, -5), MoveCooldown / 2);
		_bobTween.TweenProperty(BodySprite, "position", new Vector2(0, 0), MoveCooldown / 2);
	}
	
	private void PlayDamageAnimation()
	{
		_damageTween?.Kill();
		_damageTween = SpriteHolder.CreateTween();
		_damageTween.TweenProperty(SpriteHolder, "modulate", new Color(1, 0, 0), BattleScene.TimeStep / 5);
		_damageTween.TweenProperty(SpriteHolder, "modulate", Colors.White, BattleScene.TimeStep /  5);
	}
	
	public void PlayAttackAnimation()
	{
		// jiggle the unit
		if (_jiggleTween != null) return;
		_jiggleTween = BodySprite.CreateTween();
		_jiggleTween.SetTrans(Tween.TransitionType.Quad);
		_jiggleTween.SetEase(Tween.EaseType.Out);
		_jiggleTween.TweenProperty(SpriteHolder, "rotation_degrees", 10, BattleScene.TimeStep /  5);
		_jiggleTween.TweenProperty(SpriteHolder, "rotation_degrees", 0, BattleScene.TimeStep /  5);
		_jiggleTween.TweenCallback(Callable.From(() => _jiggleTween = null));
	}
	
	private void PlayDeathAnimation()
	{
		SpriteHolder.RemoveChild(BodySprite);
		Globals.BattleScene.AddChild(BodySprite);
		
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
		
		BodySprite.GlobalPosition = GlobalPosition;
		BodySprite.Modulate = new Color(.5f, .5f, .5f);
		BodySprite.ZIndex = -1;

		var deathTween = Globals.Tree.CreateTween();
		deathTween.SetTrans(Tween.TransitionType.Sine);
		deathTween.TweenProperty(BodySprite, "global_position", target, BattleScene.TimeStep);
		
		var deathRotationTween = Globals.Tree.CreateTween();
		deathRotationTween.SetTrans(Tween.TransitionType.Quad);
		deathRotationTween.SetEase(Tween.EaseType.In);
		deathRotationTween.TweenProperty(BodySprite, "rotation_degrees", targetDegrees, BattleScene.TimeStep);
		
		// This should clean up all of the other units on death other than their body.
		SpriteHolder.QueueFree();
		
		// These will throw warnings if started before queuefree.
		_jiggleTween?.Kill();
		_damageTween?.Kill();
		_bobTween?.Kill();
		_moveTween?.Kill();
	}
	
	private void PlayDeathSound()
	{
		_deathAudio.Play();
	}

	public void PlayHitSound()
	{
		_hitAudio.Play();
	}
	
	private void ShowText(int damage)
	{
		RichTextLabel text = new();
		Globals.BattleScene.AddChild(text);
		text.Text = "[color=red]" + damage.ToString() + "[/color]";
		text.GlobalPosition = SpriteHolder.GlobalPosition;
		text.CustomMinimumSize = new Vector2(100, 100);
		text.BbcodeEnabled = true;

		var labelTween = Globals.Tree.CreateTween();
		labelTween.TweenProperty(text, "global_position", text.GlobalPosition + new Vector2(0, -50), 1.0f);
		labelTween.TweenCallback(Callable.From(() =>
		{
			if (!GodotObject.IsInstanceValid(text)) return;
			text.QueueFree();
		}));
		
		var textColorTween = Globals.Tree.CreateTween();
		textColorTween.TweenProperty(text, "modulate", new Color(1, 1, 1, 0), 1.0f);
	}
}
