using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.map;

public partial class Battle : Node2D
{
	public BattleTileMap TileMap;
	public CanvasLayer GUI;
	public Province Provence;
	
	public override void _EnterTree()
	{
		Globals.BattleScene = this;
	}

	public override void _Ready()
	{
		base._Ready();
		Globals.BattleCamera = GetNode<Camera2D>("Camera2D");
	}
}
