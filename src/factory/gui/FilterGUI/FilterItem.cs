using Godot;
using System;
using Necromation;

public partial class FilterItem : ItemBox
{

    private ActionBarItem _actionBarItem;
    
    public void Init(string item, ActionBarItem actionBarItem)
    {
        ItemType = item;
        _actionBarItem = actionBarItem;
        CountLabel.Visible = false;
        Button.Pressed += ButtonPressed;
    } 
    
    private void ButtonPressed()
    {
        _actionBarItem.SetFilter(ItemType);
        Globals.FactoryScene.Gui.CloseGui();
    }
}
