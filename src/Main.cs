using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.map;

public partial class Main : Node2D
{
	private bool _initialized = false;
	
	public override void _EnterTree()
	{
		Globals.FactoryScene = this;
		VisibilityChanged += () =>
		{
			if (!Visible) return;
			MusicManager.PlayAmbiance();
			MapGlobals.TileMap.GetProvinces()
				.Where(province => province.Owner == "Player")
				.Select(province => province.MapPosition)
				.ToList()
				.ForEach(Globals.TileMap.AddProvence);
		};
		MusicManager.PlayAmbiance();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (!_initialized)
		{
			_initialized = true;
			var position = MapGlobals.FactoryPosition;
			Globals.Player.GlobalPosition =
				position * BuildingTileMap.TileSize * BuildingTileMap.ProvinceSize
				+ position * BuildingTileMap.TileSize * BuildingTileMap.ProvinceGap
				+ Vector2I.One * BuildingTileMap.TileSize * BuildingTileMap.ProvinceSize / 2;
			Globals.TileMap.AddProvence(position);
		}
	}
}
