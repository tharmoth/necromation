using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class BuildingTileMap : SKTileMap
{
	public enum LayerNames
	{
		Buildings,
		Resources,
	}
	
	public const int TileSize = 32;
	
	private readonly Dictionary<Vector2I, IEntity> _buildings = new Dictionary<Vector2I, IEntity>();
	private readonly Dictionary<Vector2I, IEntity> _resources = new Dictionary<Vector2I, IEntity>();
	private readonly Dictionary<LayerNames, Dictionary<Vector2I, IEntity>> _layers = new Dictionary<LayerNames, Dictionary<Vector2I, IEntity>>();
	
	public override void _EnterTree(){
		Globals.TileMap = this;
		// TODO: refactor this into a collision mask instead of discrete layers?
		_layers.Add(LayerNames.Buildings, _buildings);
		_layers.Add(LayerNames.Resources, _resources);
	}

	public void AddEntity(Vector2 position, IEntity entity, LayerNames layerName)
	{
		AddEntity(GlobalToMap(position), entity, layerName);
	}

	public void AddEntity(Vector2I position, IEntity entity, LayerNames layerName){
		if (!_layers.ContainsKey(layerName)) return;
		_layers[layerName].Add(position, entity);
		
		if (entity is not Node2D buildingNode || buildingNode.GetParent() != null) return;
		buildingNode.GlobalPosition = Globals.TileMap.MapToGlobal(position);
		GetTree().Root.AddChild(buildingNode);
	}
	
	public void RemoveEntity(Vector2 position, LayerNames layerName){
		RemoveEntity(GlobalToMap(position), layerName);
	}
	
	public bool RemoveEntity(Vector2I position, LayerNames layerName){
		if (!_layers.ContainsKey(layerName)) return false;
		if (!_layers[layerName].ContainsKey(position)) return false;
		var entity = _layers[layerName][position];
		_layers[layerName].Remove(position);
		
		if (entity is not Node2D buildingNode) return false;
		if (buildingNode.GetParent() == GetTree().Root) GetTree().Root.RemoveChild(buildingNode);
		return true;
	}

	public bool RemoveEntity(IEntity entity)
	{
		var removed = false;
		
		foreach (var (layerName, layer) in _layers)
		{
			if (!layer.ContainsValue(entity)) continue;
			removed = layer.Where(pair => pair.Value == entity)
				.Select(pair => pair.Key)
				.ToList()
				.Select(position => RemoveEntity(position, layerName))
				.All(hasBeenRemoved => hasBeenRemoved);
		}

		return removed;
	}
	
	public IEntity GetEntities(Vector2 position, LayerNames layerName)
	{
		return GetEntities(GlobalToMap(position), layerName);
	}
	
	public IEntity GetEntities(Vector2I position, LayerNames layerName)
	{
		if (!_layers.TryGetValue(layerName, out var layer) 
		    || !layer.TryGetValue(position, out var entity)) return null;
		return entity;
	}
	
	public List<IEntity> GetEntities(Vector2 position)
	{
		return GetEntities(GlobalToMap(position));
	}

	public List<IEntity> GetEntities(Vector2I position)
	{
		return _layers.Values
			.Where(layer => layer.ContainsKey(position))
			.Select(layer => layer[position])
			.ToList();
	}
	
	public List<Vector2I> GetEntityPositions(IEntity entity)
	{
		return _layers.Values
			.SelectMany(layer => layer.Where(pair => pair.Value == entity))
			.Select(pair => pair.Key)
			.ToList();
	}
	
	public bool IsEmpty(Vector2 position, LayerNames layerName)
	{
		return IsEmpty(GlobalToMap(position), layerName);
	}
	
	public bool IsEmpty(Vector2I position, LayerNames layerName)
	{
		return !_layers.ContainsKey(layerName) || !_layers[layerName].ContainsKey(position);
	}
	
	public bool IsEmpty(Vector2 position)
	{
		return IsEmpty(GlobalToMap(position));
	}
	
	public bool IsEmpty(Vector2I position)
	{
		return GetEntities(position).Count == 0;
	}

	public interface IEntity
	{
		
	}

	public interface IBuilding
	{
		public string ItemType { get; }
	}
	
	
	public IEntity GetBuildingAtMouse()
	{
		return GetEntities(GetGlobalMousePosition(), LayerNames.Buildings);
	}
	
	public IEntity GetResourceAtMouse()
	{
		return GetEntities(GetGlobalMousePosition(), LayerNames.Resources);
	}
}
