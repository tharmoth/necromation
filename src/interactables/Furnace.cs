using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.character;
using Necromation.gui;
using Necromation.interfaces;

public partial class Furnace : Assembler
{
	public override Vector2I BuildingSize => Vector2I.One * 2;
	public override string ItemType => "Stone Furnace";

	public override string GetCategory()
	{
		return "smelting";
	}
	
	protected override bool MaxOutputItemsReached()
	{
		return GetOutputInventory().CountAllItems() >= 200;
	}
}
