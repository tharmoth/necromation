using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation;

public partial class CheatCodes : Node
{
	
	private Queue<String> _keys = new();
	
	public override void _UnhandledKeyInput(InputEvent @event)
	{
		base._UnhandledKeyInput(@event);
		if (@event is InputEventKey { Pressed: true } keyEvent)
		{
			var key = keyEvent.KeyLabel.ToString();
			if (key == "Quoteleft") key = "`";
			_keys.Enqueue(key);
			
			if (_keys.Count > 16) _keys.Dequeue();

			var enteredString = _keys.Aggregate("", (s, s1) => s + s1);
			ProcessCheatCodes(enteredString);
		}
	}

	private void ProcessCheatCodes(string entered)
	{
		if (entered.EndsWith("``WORLD"))
		{
			GD.Print("All your base are belong to us");
			Globals.MapScene.TileMap.GetProvinces().ForEach(province => province.Owner = "Player");
			Globals.FactoryScene.TileMap.OnOpen();
		}
	}
}
