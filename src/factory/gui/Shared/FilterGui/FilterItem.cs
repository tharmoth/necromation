using Godot;
using System;
using Necromation;

public partial class FilterItem : ItemBox
{

    private HotBarItemBox _hotBarItemBox;
    
    public void Init(string item, HotBarItemBox hotBarItemBox)
    {
        ItemType = item;
        _hotBarItemBox = hotBarItemBox;
        CountLabel.Visible = false;
        Button.Pressed += ButtonPressed;
    } 
    
    private void ButtonPressed()
    {
        _hotBarItemBox.SetFilter(ItemType);
        Globals.FactoryScene.Gui.CloseGui();
    }
}
