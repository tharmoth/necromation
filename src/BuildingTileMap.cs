using Godot;
using System;
using System.Collections.Generic;
using Necromation;

public partial class BuildingTileMap : SKTileMap
{
	private readonly Dictionary<Vector2I, IBuilding> _buildings = new Dictionary<Vector2I, IBuilding>();
	
	public override void _EnterTree(){
		Globals.TileMap = this;
	}
	
	public bool CanBuild(Vector2 globalPosition){
		return CanBuild(GlobalToMap(globalPosition));
	}
	
	public bool CanBuild(Vector2I position){
		return !_buildings.ContainsKey(position);
	}
	
	public void Build(Vector2 globalPosition, IBuilding building){
		Build(GlobalToMap(globalPosition), building);
	}
	
	public void Build(Vector2I position, IBuilding building){
		if (!CanBuild(position)) return;
		_buildings.Add(position, building);
		
		if (building is not Node2D buildingNode) return;
		GetTree().Root.AddChild(buildingNode);
		buildingNode.GlobalPosition = Globals.TileMap.MapToGlobal(position);
		
		GD.Print("Placed building at " + position);
	}
	
	public void Remove(Vector2I position){
		if (!_buildings.ContainsKey(position)) return;
		var building = _buildings[position];
		_buildings.Remove(position);
		
		if (building is not Node2D buildingNode) return;
		GetTree().Root.RemoveChild(buildingNode);
	}
	
	public IBuilding GetBuilding(Vector2I position){
		return _buildings.ContainsKey(position) ? _buildings[position] : null;
	}
	
	public IBuilding GetBuilding(Vector2 globalPosition){
		return GetBuilding(GlobalToMap(globalPosition));
	}

	public interface IBuilding
	{
		public string ItemType { get; }
	}
}
