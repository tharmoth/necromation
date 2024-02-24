using Godot;
using System.Collections.Generic;
using System.Linq;
using Necromation.map;
using Necromation.map.gui;

public partial class CommanderUnitList : VBoxContainer
{
	List<UnitsBox> _boxes = new();
	
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
			var panel = new UnitAccepterPanel(commander.GetInventories().First());
			panel.SizeFlagsHorizontal = SizeFlags.ExpandFill; 
			var box = new UnitsBox(commander.GetInventories().First());
			_boxes.Add(box);
			Label label = new();
			label.Text = commander.Name;
			box.AddChild(label);
			panel.AddChild(box);
			AddChild(panel);
		}
	}
	
	public List<UnitRect> GetAllSelected()
	{
		return _boxes.SelectMany(box => box.GetAllSelected()).ToList();
	}
}
