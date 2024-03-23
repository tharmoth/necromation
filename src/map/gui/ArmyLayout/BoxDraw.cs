using Godot;
using System;
using Necromation;
using Necromation.map.character;

public partial class BoxDraw : Control
{
	private Sprite2D BoxSprite => GetNode<Sprite2D>("%Sprite");
	private Sprite2D Background => GetNode<Sprite2D>("%Background");
	private Vector2I _gridSize = new(50, 50);
	private Commander _commander;
	
	public static void Display(Commander commander)
	{
		var gui = GD.Load<PackedScene>("res://src/map/gui/ArmyLayout/box_draw.tscn").Instantiate<BoxDraw>();
		gui.Init(commander);
		Globals.MapScene.Gui.GuiStack.Push(gui);
		Globals.MapScene.Gui.AddChild(gui);
	}
	
	public void Init(Commander commander)
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
		
		GD.Print(globalX, " nani ", globalY);
		
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
		
		GD.Print("Grid: ", gridX, " ", gridY);

		if (!(x < 1.0f) || !(x >= 0) || !(y < 1.0f) || !(y >= 0)) return;
			
		BoxSprite.GlobalPosition = Background.GetGlobalMousePosition();
		_commander.SpawnLocation = new Vector2I(gridX, gridY);
	}
}
