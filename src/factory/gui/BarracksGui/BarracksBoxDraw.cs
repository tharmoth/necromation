using Godot;
using Necromation.map.character;
using Necromation;

public partial class BarracksBoxDraw : Control
{
	/************************************************************************
	 * Hardcoded Scene Imports 												*
	 ************************************************************************/
	private readonly static PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/BarracksGui/BarracksBoxDraw.tscn");
	
	/************************************************************************
	 * Child Accessors 													    *
	 ************************************************************************/
	private Sprite2D BoxSprite => GetNode<Sprite2D>("%Sprite");
	private Sprite2D Background => GetNode<Sprite2D>("%Background");
	
	/**************************************************************************
	 * Logic Variables                                                        *
	 **************************************************************************/
	private Commander _commander;
	
	/**************************************************************************
	 * Constants         													  *
	 **************************************************************************/
	private readonly Vector2I _gridSize = new(100, 100);
	
	public static void Display(Commander commander)
	{
		var gui = Scene.Instantiate<BarracksBoxDraw>();
		gui.Init(commander);
		Globals.FactoryScene.Gui.Push(gui);
	}
	
	private void Init(Commander commander)
	{
		_commander = commander;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Place boxsprite at the commanders spawn location
		var spawnLocation = _commander.SpawnLocation;
		
		BoxSprite.GlobalPosition = Background.GlobalPosition;
		
		var x = spawnLocation.X / (float)_gridSize.X;
		var y = spawnLocation.Y / (float)_gridSize.Y;
		
		var size = Background.GetRect().Size * Background.Scale;
		
		var globalX = size.X * x - size.X / 2;
		var globalY = size.Y * y - size.Y / 2;

		BoxSprite.GlobalPosition += new Vector2(globalX, globalY);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!Input.IsActionJustPressed("left_click")) return;
		
		// find out where on box the mouse is
		var diff = Background.GetGlobalMousePosition() - Background.GlobalPosition - Background.Offset * Background.Scale;
		var size = Background.GetRect().Size * Background.Scale;
		var x = diff.X / size.X;
		var y = diff.Y / size.Y;
		var gridX = Mathf.RoundToInt(_gridSize.X * x);
		var gridY = Mathf.RoundToInt(_gridSize.Y * y);

		if (!(x < 1.0f) || !(x >= 0) || !(y < 1.0f) || !(y >= 0)) return;
			
		BoxSprite.GlobalPosition = Background.GetGlobalMousePosition();
		_commander.SpawnLocation = new Vector2I(gridX, gridY);
	}
}