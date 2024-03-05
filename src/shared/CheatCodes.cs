using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class CheatCodes : Node
{
	
	private Queue<String> _keys = new();

	public override void _Ready()
	{
		base._Ready();
		GD.Print("Cheater!");
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		base._UnhandledKeyInput(@event);
		if (@event is InputEventKey { Pressed: true } keyEvent)
		{
			var key = keyEvent.KeyLabel.ToString();
			GD.Print(key);
			if (key == "Quoteleft") key = "`";
			if (key == "Space") key = " ";
			_keys.Enqueue(key);
			
			if (_keys.Count > 16) _keys.Dequeue();

			var enteredString = _keys.Aggregate("", (s, s1) => s + s1);
			ProcessCheatCodes(enteredString);
			GD.Print(enteredString);
		}
	}

	private void ProcessCheatCodes(string entered)
	{
		if (entered.EndsWith("``ONOTHEIST"))
		{
			GD.Print("POLYTHEIST");
			Globals.MapScene.TileMap.GetProvinces().ForEach(province => province.Owner = "Player");
			Globals.FactoryScene.TileMap.OnOpen();
		}

		if (entered.EndsWith("``POLYTHEIST"))
		{
			GD.Print("MONOTHEIST");
			Globals.MapScene.TileMap.GetProvinces().ForEach(province => province.Owner = "Enemy");
			Globals.FactoryScene.TileMap.OnOpen();
		}
		
		if(entered.EndsWith("``ROBIN HOOD"))
		{
			GD.Print("SHERIFF OF NOTTINGHAM");
			Database.Instance.Recipes
				.Select(recipe => recipe.Products.First().Key)
				.ToList()
				.ForEach(item => Globals.PlayerInventory.Insert(item, 100));
		}
		
		if (entered.EndsWith("``SHERIFF OF NOTTINGHAM"))
		{
			GD.Print("ROBIN HOOD");
			Globals.PlayerInventory.Clear();
		}
		
		if (entered.EndsWith("``GOLDEN AGE"))
		{
			GD.Print("DARK AGE");
			Database.Instance.Technologies.ToList().ForEach(tech => tech.Unlock());
		}
		
		if (entered.EndsWith("``DARK AGE"))
		{
			GD.Print("GOLDEN AGE");
			Database.Instance.Technologies.ToList().ForEach(tech => tech.Lock());
		}
	}
}
