using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Necromation.map;
using Necromation.map.gui;

public partial class CommanderUnitList : VBoxContainer
{
	public void Update()
	{
		var provence = MapGlobals.SelectedProvince;
		if (provence == null) return;
		
		GetChildren().OfType<UnitAccepterPanel>().ToList().ForEach(button =>
		{
			RemoveChild(button);
			button.QueueFree();
		});

		foreach (var commander in provence.Commanders)
		{
			var panel = new UnitAccepterPanel(commander.GetOutputInventory());
			var scroll = new ScrollContainer();
			scroll.VerticalScrollMode = ScrollContainer.ScrollMode.Disabled;
			var box = new UnitsBox(commander.GetOutputInventory());
			Label label = new();
			label.Text = commander.Name;
			box.AddChild(label);
			scroll.AddChild(box);
			panel.AddChild(scroll);
			AddChild(panel);
		}
	}
	
	public List<UnitRect> GetAllSelected()
	{
		return GetChildren().OfType<UnitAccepterPanel>()
			.SelectMany(panel => panel.GetChildren().OfType<ScrollContainer>())
			.SelectMany(panel => panel.GetChildren().OfType<UnitsBox>())
			.SelectMany(box => box.GetAllSelected()).ToList();
	}
}
