using Godot;
using System;
using Necromation;
using Necromation.map;

public partial class Main : Node2D
{
	private Province _province = null;
	
	public override void _EnterTree()
	{
		Globals.FactoryScene = this;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (_province != MapGlobals.SelectedProvince)
		{
			_province = MapGlobals.SelectedProvince;
			Globals.Player.GlobalPosition = (_province.MapPosition - MapGlobals.FactoryPosition) 
				* BuildingTileMap.TileSize * 200 + Vector2I.One * BuildingTileMap.TileSize * 50;
			GD.Print(_province.MapPosition);
			GD.Print(Globals.Player.GlobalPosition);
		}
	}
}
