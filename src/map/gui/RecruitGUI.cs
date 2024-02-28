using Godot;
using System.Linq;
using Necromation.map;

public partial class RecruitGUI : Control
{
	public override void _Ready()
	{
		GetNode<Button>("%CommanderButton").Pressed += RecruitCommander;
		GetNode<Button>("%SoldierButton").Pressed += RecruitSoldier;
		GetNode<Button>("%ArcherButton").Pressed += RecruitArcher;
		GetNode<Button>("%HorseButton").Pressed += RecruitHorse;
		
		MapGlobals.UpdateListeners.Add(Update);
	}

	private void RecruitCommander()
	{
		Recruit("commander");
	}

	private void RecruitSoldier()
	{
		Recruit("Infantry");
	}

	private void RecruitArcher()
	{
		Recruit("Archer");
	}

	private void RecruitHorse()
	{
		Recruit("horse");
	}

	private void Recruit(string type)
	{
		MapGlobals.SelectedProvince?.Recruit(type);
		Update();
	}

	private void Update()
	{
		var provence = MapGlobals.SelectedProvince;
		if (provence == null) return;
		
		var label = GetNode<Label>("%RecruitList");
		label.Text = "";
		foreach (var (name, count) in provence.RecruitQueue.OrderBy(pair => pair.Key))
		{
			label.Text += "Recruit " + name + " x" + count + "\n";
		}
	}
}
