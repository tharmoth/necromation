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
		Resources
	}
	
	private readonly Dictionary<Vector2I, IEntity> _buildings = new Dictionary<Vector2I, IEntity>();
	private readonly Dictionary<Vector2I, IEntity> _resources = new Dictionary<Vector2I, IEntity>();
	
	private readonly Dictionary<LayerNames, Dictionary<Vector2I, IEntity>> _layers = new Dictionary<LayerNames, Dictionary<Vector2I, IEntity>>();
	
	public override void _EnterTree(){
		Globals.TileMap = this;
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
		
		if (entity is not Node2D buildingNode) return;
		GetTree().Root.AddChild(buildingNode);
		buildingNode.GlobalPosition = Globals.TileMap.MapToGlobal(position);
	}
	
	public void RemoveEntity(Vector2 position, LayerNames layerName){
		RemoveEntity(GlobalToMap(position), layerName);
	}
	
	public void RemoveEntity(Vector2I position, LayerNames layerName){
		if (!_layers.ContainsKey(layerName)) return;
		if (!_layers[layerName].ContainsKey(position)) return;
		var entity = _layers[layerName][position];
		_layers[layerName].Remove(position);
		
		if (entity is not Node2D buildingNode) return;
		GetTree().Root.RemoveChild(buildingNode);
	}

	public void RemoveEntity(IEntity entity)
	{
		foreach (var (layerName, layer) in _layers)
		{
			if (!layer.ContainsValue(entity)) return;
			var position = layer.First(pair => pair.Value == entity).Key;
			RemoveEntity(position, layerName);
		}
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
		public bool CanRemove();
	}
}
