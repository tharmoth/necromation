using Godot;
using System;
using Necromation;

public partial class HelpGui : PanelContainer
{
	/**************************************************************************
	 * Hardcoded Scene Imports 											      *
	 **************************************************************************/
	private static readonly PackedScene Scene = GD.Load<PackedScene>("res://src/factory/gui/HelpGui/HelpGui.tscn");

	/**************************************************************************
	 * Child Accessors 													      *
	 **************************************************************************/
	private Container InventoryItemList => GetNode<Container>("%InventoryItemList");
	private RichTextLabel RichTextLabel => GetNode<RichTextLabel>("%RichTextLabel");
	private Label Title => GetNode<Label>("%Title");
	
	
	// Static Accessor
	public static void Display(bool factory, bool victory = false)
	{
		var gui = Scene.Instantiate<HelpGui>();

		if (victory)
		{
			gui.Init(VictoryHelp, victory);
			Globals.MapScene.Gui.Open(gui);
		}
		else if (factory)
		{
			gui.Init(MainHelp);
			Globals.FactoryScene.Gui.Open(gui);
		}
		else
		{
			gui.Init(MapHelp);
			Globals.MapScene.Gui.Open(gui);
		}
			
	}

	private void Init(string helpText, bool victory = false)
	{
		if (victory)
		{
			Title.Text = "Dragon Slain!";
		}
		RichTextLabel.Text = helpText;
	}

	private const string MainHelp =
		"[center]Welcome to Necromation![/center]\n\nBuild troops and place them in a barracks.\nCommand the troops in the map to conquer new lands and expand the crypt!\nAutomate the production of your armies.\nSlay the dragon of the Flamecrest Peninsula far to the south!\n\nKeyboard Shortcuts\n[ul]\nW/A/S/D: Move your character.\n[/ul]\n\nInteraction\n[ul]\nLeft Click - Open building GUI\nRight Click - Hold to mine ore\nCtrl + Left Click - Place selected item in building or Remove items from building\n[/ul]\n\nBuilding Placement\n[ul]\nLeft Click - Place Selected Building\nRight Click - Hold to remove hovered building\nQ - Clear Selection\nQ - Select Hovered Building\nR - Rotate Selected building\nAlt + D - Open Mass building deconstruction\n[/ul]\n\nCrafting:\n[ul]\nLeft Click: Craft\nRight Click: Craft 5\nShift + Left Click: Craft All\n[/ul]\n\nMenus and UI:\n[ul]\nE: Open Inventory\nM: Open Map\nEsc: Close Current GUI\nLeft Click: Select Item\nShift + Left Click: Transfer All\nCtrl + Left Click: Transfer 5\n[/ul]\n\n";

	private const string MapHelp =
		"[center]Welcome to Necromation![/center]\n\nUnits placed in barracks will appear as squads on the Map\n\nSelect a province in your domain.\nSelect the units you wish to command\nCommand the squads to move by right clicking another province.\nPress the move squads button to execute your orders\n\nSquad Command\n[ul]\nN - Execute your squads current orders\nLeft Click - Select a squad\nRight Click - Move selected squads to province\nCtrl + Left Click - Add squad to selection\nShift + Left Click - Add squads to selection\n[/ul]\n\nCamera Controls\n[ul]\nW/A/S/D: Move your character.\nScroll Wheel: Zoom\n[/ul]\n\nHotkeys\n[ul]\nEsc - Return to factory\nM - Return to factory\n[/ul]";
	
	private const string VictoryHelp =
		"[center]Congratulations!\n\nYou have slain the dragon of the Flamecrest Peninsula!\n\nYou have conquered the land and the crypt is yours!\n\nPost a Screenshot in the Necromation Discord to claim your title as conquerer of Flamecrest.\n\n[center]Thank you for playing the Necromation demo![/center]";
}
