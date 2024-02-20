using Godot;

namespace Necromation;

public partial class ResearchLab : StoneChest
{
    public override Vector2I BuildingSize => Vector2I.One * 3;
    public override string ItemType => "Research Lab";

    private bool _isResearching;
    private double _researchedAmount = 0;

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Globals.CurrentTechnology == null)
        {
            _isResearching = false;
            return;
        }

        if (_isResearching)
        {
            _researchedAmount += delta / 30;
            Globals.CurrentTechnology.Progress += delta / 30;
            if (_researchedAmount < 1.0f) return;
            _isResearching = false;
            _researchedAmount = 0;
            return;
        }

        if (!GetItems().Contains("Experiment")) return;
        Remove("Experiment");
        _isResearching = true;
    }
    
    public override bool CanAcceptItems(string item, int count = 1)
    {
        return item == "Experiment" && GetInventories().TrueForAll(inventory => inventory.CountItem(item) < 10);
    }
}