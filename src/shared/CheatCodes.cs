using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class CheatCodes : Node
{
	
	private readonly Queue<string> _keys = new();

	public override void _Ready()
	{
		base._Ready();
		// if (true) return;
		if (OS.IsDebugBuild()) Add100AllItems();
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		base._UnhandledKeyInput(@event);
		if (@event is not InputEventKey { Pressed: true } keyEvent) return;
		
		var key = keyEvent.KeyLabel.ToString();
		if (key == "Quoteleft") key = "`";
		if (key == "Space") key = " ";
		_keys.Enqueue(key);
			
		if (_keys.Count > 16) _keys.Dequeue();

		var enteredString = _keys.Aggregate("", (s, s1) => s + s1);
		ProcessCheatCodes(enteredString);
	}

	private void ProcessCheatCodes(string entered)
	{
		if (entered.EndsWith("``ONOTHEIST"))
		{
			GD.Print("POLYTHEIST");
			Globals.MapScene.Map.Provinces.ForEach(province => province.Owner = "Player");
			Globals.FactoryScene.TileMap.OnOpen();
		}

		if (entered.EndsWith("``POLYTHEIST"))
		{
			GD.Print("MONOTHEIST");
			Globals.MapScene.Map.Provinces.ForEach(province => province.Owner = "Enemy");
			Globals.FactoryScene.TileMap.OnOpen();
		}

		if (entered.EndsWith("``ROBIN")) Add100AllItems();
		
		if (entered.EndsWith("``SHERIFF"))
		{
			GD.Print("ROBIN HOOD");
			Globals.PlayerInventory.Clear();
		}
		
		if (entered.EndsWith("``GOLDEN AGE"))
		{
			GD.Print("DARK AGE");
			Database.Instance.Technologies.ToList().ForEach(tech => tech.Research());
		}
		
		if (entered.EndsWith("``DARK AGE"))
		{
			GD.Print("GOLDEN AGE");
			Database.Instance.Technologies.ToList().ForEach(tech => tech.UnResearch());
		}
		
		if (entered.EndsWith("``FPS"))
		{
			Globals.FactoryScene.Gui.ToggleFps();
		}
	}

	private void Add100AllItems()
	{
		GD.Print("SHERIFF OF NOTTINGHAM");
		Database.Instance.Recipes
			.Select(recipe => recipe.Products.First().Key)
			.ToList()
			.ForEach(item => Globals.PlayerInventory.Insert(item, 100));
			
		Globals.PlayerInventory.Insert("Coal Ore", 100);
		Globals.PlayerInventory.Insert("Bone Fragments", 100);
		Globals.PlayerInventory.Insert("Copper Ore", 100);
		Globals.PlayerInventory.Insert("Stone", 100);
		Globals.PlayerInventory.Insert("Tin Ore", 100);
		Globals.PlayerInventory.Insert("Void Chest", 100);
		Globals.PlayerInventory.Insert("Infinite Chest", 100);
		Globals.PlayerInventory.Insert("Loader", 100);
	}
}
