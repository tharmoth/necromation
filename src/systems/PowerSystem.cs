using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Necromation;

/// <summary>
/// This system is responsible for managing power networks. It detects when pylons are placed and calculates all 
/// the connected networks of pylons. It stores the networks and updates them every frame.
/// </summary>
public partial class PowerSystem : Node
{
    // Networks are stored in a hash set to prevent duplicates.
    private readonly HashSet<PowerNetwork> _networks = new();
    
    public override void _EnterTree()
    {
        base._Ready();
        Globals.FactoryScene.TileMap.listeners.Add(Update);
        Update();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Globals.FactoryScene.TileMap.listeners.Remove(Update);
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        foreach (var network in _networks)
        {
            network._Process(delta);
        }
    }
    
    private void Update()
    {
        var buildings = Globals.FactoryScene.TileMap.GetEntities(FactoryTileMap.Building)
            .ToHashSet();
        
        // Calculate all the networks of pylons on the map.
        var pylons = buildings.OfType<Pylon>().ToHashSet();
        pylons.ToList().ForEach(pylon => pylon.Update());
        _networks.Clear();
        _networks.UnionWith(FindNetworks(pylons));
        
        // Find all the power sources and consumers on the map.
        var powerSources = buildings.OfType<IPowerSource>().ToHashSet();
        var powerConsumers = buildings.OfType<IPowerConsumer>().ToHashSet();

        // Find all the power sources and consumers that are connected to a network.
        var connectedSources = _networks.SelectMany(network => network.Sources).ToHashSet();
        var connectedConsumers = _networks
            .Where(network => network.HasPower)
            .SelectMany(network => network.Consumers)
            .ToHashSet();
        
        // Disconnect the sources not in a network.
        powerSources.Except(connectedSources).ToList().ForEach(source => source.Disconnected = true);
        powerConsumers.Except(connectedConsumers).ToList().ForEach(consumer => consumer.Disconnected = true);
        
        // Connect the sources and consumers in a network.
        connectedSources.ToList().ForEach(source => source.Disconnected = false);
        connectedConsumers.ToList().ForEach(consumer => consumer.Disconnected = false);
    }
    
    private static HashSet<PowerNetwork> FindNetworks(HashSet<Pylon> pylons)
    {
        var visited = new HashSet<Pylon>();
        var networks = new HashSet<PowerNetwork>();
        foreach (var pylon in pylons)
        {
            if (visited.Contains(pylon)) continue;
            var network = new PowerNetwork();
            DepthFirstSearch(pylon, visited, network.Links);
            networks.Add(network);
        }

        foreach (var network in networks)
        {
            network.Consumers.UnionWith(network.Links.SelectMany(pylon => pylon.Consumers));
            network.Sources.UnionWith(network.Links.SelectMany(pylon => pylon.Sources));
        }
        
        return networks;
    }
    
    private static void DepthFirstSearch(Pylon pylon, HashSet<Pylon> visited, HashSet<Pylon> network)
    {
        visited.Add(pylon);
        network.Add(pylon);
        foreach (var link in pylon.Links)
        {
            if (visited.Contains(link)) continue;
            DepthFirstSearch(link, visited, network);
        }
    }
}

/// <summary>
/// A power network is a collection of pylons that are connected to each other. It manages distributing power
/// from the connected IPowerSources to the connected  IPowerConsumers.
/// </summary>
public class PowerNetwork
{
    public readonly HashSet<IPowerSource> Sources = new();
    public readonly HashSet<IPowerConsumer> Consumers = new();
    public readonly HashSet<Pylon> Links = new();
    public bool HasPower => Sources.Count > 0;
    
    public void _Process(double delta)
    {
        var energyRequired = Consumers.Sum(consumer => consumer.EnergyRequired);
        var maxEnergyAvailable = Sources.Sum(source => Math.Min(source.EnergyStored, source.PowerLimit * (float) delta));
        var energyConsumed = 0f;

        foreach (var consumer in Consumers)
        {
            var energyToConsume = Math.Min(consumer.EnergyRequired, consumer.PowerLimit * (float)delta);
            energyToConsume = Math.Min(energyToConsume, maxEnergyAvailable - energyConsumed);
            energyConsumed += energyToConsume;
            consumer.EnergyStored += energyToConsume;
        }
        
        var energyProvided = 0f;
        foreach (var source in Sources)
        {
            var powerToProvide = Math.Min(source.EnergyStored, source.PowerLimit * (float)delta);
            powerToProvide = Math.Min(powerToProvide, energyRequired - energyProvided);
            energyProvided += powerToProvide;
            source.EnergyStored -= powerToProvide;
        }

        Debug.Assert(energyProvided >= energyConsumed,
            "We should always provide more power than we consume!");
    }
}

public interface IPowerConsumer
{
    /// <summary>
    /// The amount of energy stored in the entity available for use.
    /// </summary>
    public float EnergyStored { get; set; }
    /// <summary>
    /// The maximum amount of energy that can be stored in the entity.
    /// </summary>
    public float PowerMax { get; }
    /// <summary>
    /// The maximum amount of power that can be input into the entity per second.
    /// </summary>
    public float PowerLimit { get; }
    /// <summary>
    /// Amount of power required by the entity to reach full power.
    /// </summary>
    public float EnergyRequired => PowerMax - EnergyStored;
    /// <summary>
    /// If the entity is connected to a network.
    /// </summary>
    public bool Disconnected { set; }
}

public interface IPowerSource
{
    /// <summary>
    /// The amount of energy stored in the entity available for use.
    /// </summary>
    public float EnergyStored { get; set; }
    /// <summary>
    /// The maximum amount of power that can be output from the entity per second.
    /// </summary>
    public float PowerLimit { get; }
    /// <summary>
    /// If the entity is connected to a network.
    /// </summary>
    public bool Disconnected { set; }
}