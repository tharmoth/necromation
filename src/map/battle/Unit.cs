using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.character;
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
	private string _unitType;
	private Weapon _weapon;
	
	private int _hp = 100;

	public double Cooldown = GD.RandRange(0, 1.0);
	private Tween _jiggleTween;
	private Tween _bobTween;
	private Tween _moveTween;
	private Tween _damageTween;

	protected Sprite2D Sprite = new();
	private List<Unit> enemies = null;
	public AudioStreamPlayer2D Audio = new AudioStreamPlayer2D();

	public Unit(string unitType, Commander commander = null)
	{
		_unitType = unitType;
		_commander = commander;
		_weapon = unitType switch
		{
			"Archer" => new RangedWeapon(this, 100, 49),
			"Warrior" => new MeleeWeapon(this, 1, 52),
			_ => throw new NotImplementedException()
		};

		// Sprite.Texture = Globals.Database.GetTexture(_unitType);
		Sprite.Texture = unitType switch
		{
			"Archer" => GD.Load<Texture2D>("res://res/sprites/Archer.png"),
			"Warrior" => GD.Load<Texture2D>("res://res/sprites/SpearMan.png"),
			_ => throw new NotImplementedException()
		};
		Sprite.Scale = new Vector2(.125f, .125f);
		// Sprite.Scale = new Vector2(.5f, .5f);
		AddChild(Sprite); 
		AddChild(Audio);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Sprite.FlipH = Team == "Player";
		// Sprite.Modulate = Team == "Player" ? new Color(.8f, .8f, 1) : new Color(1, .8f, .8f);
		AddToGroup("Units");
		CachedPosition = GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		enemies ??= GetTree()
			.GetNodesInGroup("Units")
			.OfType<Unit>()
			.Where(unit => unit.Team != Team).ToList();
		enemies.RemoveAll(unit => !IsInstanceValid(unit));
		
		// Every 0.5 seconds, move the unit one cell forward
		Cooldown += delta;
		if (Cooldown < Battle.TimeStep || _hp <= 0) return;
		Cooldown = 0;

		if (_weapon.CanAttack(enemies))
		{
			_weapon.Attack();
			return;
		}

		var rand = GD.Randf();
		if (rand < 0.25)
		{
			TargetRandomEnemy();
		}
		else if (rand < 0.5)
		{
			TargetClosestEnemy();
		}

		MoveToTarget();
	}
	
	/**************************************************************************
	 * Public Methods                                                         *
	 **************************************************************************/
	public void Damage(int damage)
	{
		_damageTween?.Kill();
		_damageTween = CreateTween();
		_damageTween.TweenProperty(this, "modulate", new Color(1, 0, 0), Battle.TimeStep / 5);
		_damageTween.TweenProperty(this, "modulate", Colors.White, Battle.TimeStep /  5);

		RichTextLabel damageText = new();
		Globals.BattleScene.AddChild(damageText);
		damageText.Text = "[color=red]" + damage.ToString() + "[/color]";
		damageText.GlobalPosition = GlobalPosition;
		damageText.CustomMinimumSize = new Vector2(100, 100);
		damageText.BbcodeEnabled = true;

		var labelTween = GetTree().CreateTween();
		labelTween.TweenProperty(damageText, "global_position", damageText.GlobalPosition + new Vector2(0, -50), 1.0f);
		labelTween.TweenCallback(Callable.From(() => damageText.QueueFree()));
		
		_hp -= damage;
		if (_hp > 0) return;

		PlayDeathAnimation();
		// PlayDeathSound();

		Globals.BattleScene.TileMap.RemoveEntity(this);
		_commander?.Remove(_unitType);
		QueueFree();
	}
	
	public void Jiggle()
	{
		// jiggle the unit
		if (_jiggleTween != null) return;
		_jiggleTween = CreateTween();
		_jiggleTween.SetTrans(Tween.TransitionType.Quad);
		_jiggleTween.SetEase(Tween.EaseType.Out);
		_jiggleTween.TweenProperty(this, "rotation_degrees", 10, Battle.TimeStep /  5);
		_jiggleTween.TweenProperty(this, "rotation_degrees", 0, Battle.TimeStep /  5);
		_jiggleTween.TweenCallback(Callable.From(() => _jiggleTween = null));
	}
	/**************************************************************************
	 * Private Methods                                                        *
	 **************************************************************************/
	private void TargetClosestEnemy()
	{
		var closest = enemies.MinBy(unit => unit.GlobalPosition.DistanceSquaredTo(GlobalPosition));
		TargetPosition = Globals.BattleScene.TileMap.GlobalToMap(closest?.GlobalPosition ?? GlobalPosition);
	}

	private void TargetRandomEnemy()
	{
		if (enemies.Count == 0)
		{
			TargetPosition = MapPosition;
			return;
		}
		var closest  = enemies.ElementAt(GD.RandRange(0, enemies.Count - 1));
		TargetPosition = closest?.MapPosition ?? MapPosition;
	}

	private void MoveToTarget()
	{
		if (MapPosition == TargetPosition) return;
		if (Globals.BattleScene.TileMap.IsSurrounded(MapPosition)) return;
		
		// var nextPosition = Globals.BattleScene.TileMap.GetNextPath(MapPosition, TargetPosition);
		var differance = TargetPosition - MapPosition;
		var nextPosition = MapPosition;
		
		// nextPosition 
		// nextPosition = Globals.BattleScene.TileMap.GetNextPath(MapPosition, TargetPosition);
		if (nextPosition == MapPosition)
		{
			if (differance.X > 0 && Globals.BattleScene.TileMap.IsEmpty(MapPosition + new Vector2I(1, 0)))
			{
				nextPosition = (MapPosition + new Vector2I(1, 0));
			}
			else if (differance.X < 0 && Globals.BattleScene.TileMap.IsEmpty(MapPosition + new Vector2I(-1, 0)))
			{
				nextPosition = (MapPosition + new Vector2I(-1, 0));
			}
			else if (differance.Y > 0 && Globals.BattleScene.TileMap.IsEmpty(MapPosition + new Vector2I(0, 1)))
			{
				nextPosition = (MapPosition + new Vector2I(0, 1));
			}
			else if (differance.Y < 0 && Globals.BattleScene.TileMap.IsEmpty(MapPosition + new Vector2I(0, -1)))
			{
				nextPosition = (MapPosition + new Vector2I(0, -1));
			}
		}

		if (nextPosition == MapPosition) return;

		if (!Globals.BattleScene.TileMap.IsEmpty(nextPosition)) return;
		Globals.BattleScene.TileMap.RemoveEntity(this);
		Globals.BattleScene.TileMap.AddEntity(nextPosition, this, BattleTileMap.Unit);

		_moveTween?.Kill();
		_moveTween = CreateTween();
		_moveTween.TweenProperty(this, "global_position", Globals.BattleScene.TileMap.MapToGlobal(nextPosition), Battle.TimeStep);

		_bobTween?.Kill();
		_bobTween = CreateTween();
		_bobTween.TweenProperty(Sprite, "position", new Vector2(0, -5), Battle.TimeStep / 2);
		_bobTween.TweenProperty(Sprite, "position", new Vector2(0, 0), Battle.TimeStep / 2);
		CachedPosition = Globals.BattleScene.TileMap.MapToGlobal(nextPosition);
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
		deathTween.TweenProperty(Sprite, "global_position", target, Battle.TimeStep);
		
		var deathRotationTween = GetTree().CreateTween();
		deathRotationTween.SetTrans(Tween.TransitionType.Quad);
		deathRotationTween.SetEase(Tween.EaseType.In);
		deathRotationTween.TweenProperty(Sprite, "rotation_degrees", targetDegrees, Battle.TimeStep);
	}

	private void PlayDeathSound()
	{
		var randomizer = new AudioStreamRandomizer();
		var files = DirAccess.Open("res://res/sfx/death/").GetFiles().Where(file => file.GetExtension() == "wav")
			.ToArray();
		var file = files[GD.RandRange(0, files.Length - 1)];
		randomizer.AddStream(0, GD.Load<AudioStream>("res://res/sfx/death/" + file));
		
		var audio = new AudioStreamPlayer2D();
		audio.Stream = randomizer;
		audio.VolumeDb = -40;
		Sprite.AddChild(audio);
		audio.Play();
	}
}
