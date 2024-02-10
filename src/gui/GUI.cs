using Godot;
using System;
using Necromation.gui;

public partial class GUI : CanvasLayer
{
	private static GUI _instance;
	public static GUI Instance => _instance;
	public RecipePopup Popup => GetNode<RecipePopup>("%Popup");
	public CrafterGUI CrafterGui => GetNode<CrafterGUI>("%CrafterGUI");
	public ContainerGUI ContainerGui => GetNode<ContainerGUI>("%ContainerGUI");
	
	// Use _EnterTree to make sure the Singleton instance is avaiable in _Ready()
	public override void _EnterTree(){
		if(_instance != null){
			this.QueueFree(); // The Singleton is already loaded, kill this instance
		}
		_instance = this;
	}
	
}
