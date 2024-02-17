using Godot;
using System.Collections.Generic;
using System.Linq;
using Necromation.map;
using Necromation.map.gui;

public partial class UnitsBox : HBoxContainer
{

	private List<UnitRect> _unitRects = new();
	private Inventory _inventory;

	public UnitsBox()
	{

	}
	
	public UnitsBox(Inventory inventory) : this()
	{
		_inventory = inventory;
	}

	public override void _Ready()
	{
		Update();
	}

	public void Update()
	{
		Update(_inventory);
	}

	public void Update(Inventory newInventory)
	{
		GetChildren().OfType<TextureRect>().ToList().ForEach(button =>
		{
			RemoveChild(button);
			button.QueueFree();
		});
		_unitRects.Clear();

		_inventory = newInventory;
		
		var prov = MapGlobals.SelectedProvince;
		if (prov == null) return;
		_inventory ??= MapGlobals.SelectedProvince.GetOutputInventory();
		if (_inventory == null) return;
		
		foreach (var (name, count) in _inventory.Items)
		{
			for (int i = 0; i < count; i++)
			{
				var texture = new UnitRect(name, _inventory);
				_unitRects.Add(texture);
				texture.DoubleClicked += (signalUnit) =>
				{
					_unitRects
						.Where(unit => unit.UnitName == signalUnit.UnitName)
						.ToList()
						.ForEach(unit => unit.SetSelected(signalUnit.Selected));
				};
				AddChild(texture);
			}
		}
	}
	
	public List<UnitRect> GetAllSelected()
	{
		return _unitRects.Where(unit => unit.Selected).ToList();
	}
	
	public Inventory GetInventory()
	{
		return _inventory;
	}
}
