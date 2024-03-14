using Godot;
using System;
using Necromation;
using Necromation.interactables.interfaces;

public partial class CursorManager : Node
{
	/**************************************************************************
	 * Child Accessors 													      *
	 **************************************************************************/
	private Sprite2D CursorItemSprite => GetNode<Sprite2D>("%CursorItemSprite");
	private Sprite2D CursorBuildingSprite => GetNode<Sprite2D>("%CursorBuildingSprite");
	private Sprite2D CursorEntitySprite => GetNode<Sprite2D>("%CursorEntitySprite");
	private Label CursorItemCount => GetNode<Label>("%CursorItemCount");
	
	/**************************************************************************
	 * State Data          													  *
	 **************************************************************************/
	private bool _cursorOverGui = false;
	private Tween _cursorColorTween;
	private Building _buildingInHand;
	private string _cachedSelected = "";

	public override void _Ready()
	{
		base._Ready();
		Globals.PlayerInventory.Listeners.Add(UpdateLabel);
	}

	private void UpdateLabel()
	{
		var count = Globals.Player.Selected != null ? Globals.PlayerInventory.CountItem(Globals.Player.Selected) : 0;
		CursorItemCount.Text = count != 0 ? count.ToString() : "";
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (Globals.Player.Selected == _cachedSelected) return;
		_cachedSelected = Globals.Player.Selected;
		UpdateSelected();
	}
	
	private void UpdateSelected()
	{
		if (Globals.Player.Selected != null)
		{
			CursorItemSprite.Texture = Database.Instance.GetTexture(Globals.Player.Selected);
			CursorItemSprite.Scale = new Vector2(32 / (float)CursorItemSprite.Texture.GetWidth(),
				32 / (float)CursorItemSprite.Texture.GetHeight());
		}
		
		if (Building.IsBuilding(Globals.Player.Selected))
		{
			CursorBuildingSprite.Texture = Database.Instance.GetTexture(Globals.Player.Selected);
			_buildingInHand = Building.GetBuilding(Globals.Player.Selected, IRotatable.BuildingOrientation.NorthSouth);
			if (_buildingInHand is not IRotatable) Globals.Player.SelectionRotationDegrees = 0;
		}
		else
		{
			_buildingInHand = null;
		}

		UpdateLabel();
	}
	
	public override void _Input(InputEvent @event)
	{
		UpdatePosition();
	}

	private void UpdatePosition()
	{
		CursorItemCount.Position = Globals.Player.GetGlobalMousePosition() + new Vector2(16, 16);
		// Process mouseover
		if (Globals.Player.Selected != null && Building.IsBuilding(Globals.Player.Selected) && !_cursorOverGui) SelectedBuildingPreview();
		else if (Globals.Player.Selected != null) SelectedItemPreview();
		else
		{
			CursorItemSprite.Visible = false;
			CursorBuildingSprite.Visible = false;
		}
		
		if (!_cursorOverGui && Globals.FactoryScene.TileMap.GetBuildingAtMouse() != null || Globals.FactoryScene.TileMap.GetResourceAtMouse() != null) MouseoverEntity();
		else
		{
			CursorEntitySprite.Visible = false;
			_cursorColorTween?.Kill();
			_cursorColorTween = null;
		}

		// Mark that the cursor is over a gui. Will be unmarked if it is not over a gui in UnhandledInput.
		_cursorOverGui = true;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		_cursorOverGui = false;
	}
	
	private void SelectedItemPreview()
	{
		CursorBuildingSprite.Visible = false;
		_cursorColorTween?.Kill();
		_cursorColorTween = null;
		CursorItemSprite.Visible = true;
		CursorItemSprite.RotationDegrees = Globals.Player.SelectionRotationDegrees;
		CursorItemSprite.Modulate = Colors.White;
		CursorItemSprite.Position = Globals.Player.GetGlobalMousePosition();
	}

	private void SelectedBuildingPreview()
	{
		if (!Building.IsBuilding(Globals.Player.Selected)) return;
		CursorItemSprite.Visible = false;
		_cursorColorTween?.Kill();
		_cursorColorTween = null;
		CursorBuildingSprite.Visible = true;
		CursorBuildingSprite.RotationDegrees = Globals.Player.SelectionRotationDegrees;
		CursorBuildingSprite.Modulate = Colors.White;
		CursorBuildingSprite.GlobalPosition = Globals.FactoryScene.TileMap.ToMap(Globals.Player.GetGlobalMousePosition());
		
		if (_buildingInHand is IRotatable rotatable)
			rotatable.Orientation = IRotatable.GetOrientationFromDegrees(Globals.Player.SelectionRotationDegrees);
		
		if (_buildingInHand.BuildingSize.X % 2 == 0) CursorBuildingSprite.Position += new Vector2(16, 0);
		if (_buildingInHand.BuildingSize.Y % 2 == 0) CursorBuildingSprite.Position += new Vector2(0, 16);
		
		CursorBuildingSprite.Modulate = _buildingInHand.CanPlaceAt(Globals.Player.GetGlobalMousePosition())
			? new Color(0, 1, 0, 0.5f)
			: new Color(1, 0, 0, 0.5f);
	}
	
	private void MouseoverEntity()
	{
		var buildingAtMouse = Globals.FactoryScene.TileMap.GetBuildingAtMouse();
		if (buildingAtMouse != null)
		{
			CursorEntitySprite.Scale = new Vector2(buildingAtMouse.BuildingSize.X, buildingAtMouse.BuildingSize.Y);
			CursorEntitySprite.Position = buildingAtMouse.GlobalPosition + buildingAtMouse.GetSpriteOffset();
		}
		else
		{
			CursorEntitySprite.Scale = Vector2.One;
			CursorEntitySprite.Position = Globals.FactoryScene.TileMap.ToMap(Globals.Player.GetGlobalMousePosition());
		}
		
		CursorEntitySprite.Visible = true;
		CursorEntitySprite.Modulate = Colors.White;

		if (_cursorColorTween != null) return;
		TweenColor();
	}

	private void TweenColor()
	{
		_cursorColorTween = GetTree().CreateTween();
		_cursorColorTween.TweenProperty(CursorEntitySprite, "modulate", Colors.Transparent, 0.5f);
		_cursorColorTween.TweenProperty(CursorEntitySprite, "modulate", Colors.White, 0.5f);
		_cursorColorTween.TweenCallback(Callable.From(TweenColor));
	}
}
