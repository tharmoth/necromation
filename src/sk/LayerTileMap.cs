using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Necromation.sk;

public partial class LayerTileMap : SKTileMap
{
	private readonly Dictionary<string, Dictionary<Vector2I, IEntity>> _layers = new();
	
	// TileMap listeners are fired whenever an entity is added or removed. We may need to make this more localized at some point.
	public List<Action> listeners = new();

	public void AddLayer(string layerName)
	{
		_layers.Add(layerName, new Dictionary<Vector2I, IEntity>());
	}
	
	public void AddEntity(Vector2 position, IEntity entity, string layerName)
	{
		AddEntity(GlobalToMap(position), entity, layerName);
	}

	public virtual bool AddEntity(Vector2I position, IEntity entity, string layerName){
		if (!_layers.ContainsKey(layerName)) return false;
		_layers[layerName].Add(position, entity);
		listeners.ForEach(listener => listener());
		return true;
	}
	
	public void RemoveEntity(Vector2 position, string layerName){
		RemoveEntity(GlobalToMap(position), layerName);
	}
	
	public virtual bool RemoveEntity(Vector2I position, string layerName){
		if (!_layers.ContainsKey(layerName)) return false;
		if (!_layers[layerName].ContainsKey(position)) return false;
		var entity = _layers[layerName][position];
		_layers[layerName].Remove(position);
		listeners.ForEach(listener => listener());
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
	
	public List<IEntity> GetEntities(string layerName)
	{
		return _layers.TryGetValue(layerName, out var layer) ? layer.Values.ToList() : new List<IEntity>();
	}
	
	public IEntity GetEntity(Vector2 position, string layerName)
	{
		return GetEntity(GlobalToMap(position), layerName);
	}
	
	public IEntity GetEntity(Vector2I position, string layerName)
	{
		if (!_layers.TryGetValue(layerName, out var layer) 
		    || !layer.TryGetValue(position, out var entity)) return null;
		return entity;
	}

	public List<IEntity> GetEntities(Vector2 position)
	{
		return GetEntities(GlobalToMap(position));
	}

	public List<IEntity> GetEntitiesOfType(string clazz)
	{
		return _layers.Values
			.SelectMany(layer => layer.Values)
			.Where(entity => entity is Building building && building.GetType().Name == clazz)
			.Distinct()
			.ToList();
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
	
	public bool IsEmpty(Vector2 position, string layerName)
	{
		return IsEmpty(GlobalToMap(position), layerName);
	}
	
	public bool IsEmpty(Vector2I position, string layerName)
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
	
	public virtual bool IsOnMap(Vector2I mapPos)
	{
		return GetCellSourceId(0, mapPos) != -1;
	}

	public interface IEntity
	{
		
	}
}