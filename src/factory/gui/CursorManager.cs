using Godot;
using System;
using System.Linq;
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
	private bool _motionEventHandled = false;
	private Tween _cursorColorTween;
	private Building _buildingInHand;
	private string _cachedSelected = "";

	/*************************************************************************
	 * Godot Methods                                                         *
	 *************************************************************************/
	public override void _Ready()
	{
		base._Ready();
		ProcessMode = ProcessModeEnum.Pausable;
		Globals.PlayerInventory.Listeners.Add(UpdateLabel);
	}
	
	public override void _Process(double delta)
	{
		base._Process(delta);

		UpdateCursorPosition();
		
		if (Globals.Player.Selected == _cachedSelected) return;
		_cachedSelected = Globals.Player.Selected;
		UpdateSelected();
	}
	
	public override void _Input(InputEvent @event)
	{
		UpdateCursorSprites();

		if (@event is not InputEventMouseMotion) return;
		_motionEventHandled = true;
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		base._UnhandledInput(@event);
		if (@event is not InputEventMouseMotion) return;
		_motionEventHandled = false;
	}

	/*************************************************************************
	 * Private Methods                                                       *
	 *************************************************************************/
	private bool ShouldShowPowerRange =>
		_buildingInHand != null 
		&& (_buildingInHand.GetComponent<IPowerConsumer>() != null
		    || _buildingInHand.GetComponent<IPowerSource>() != null
		    || _buildingInHand is Pylon
		);

	private void UpdateSelected()
	{
		if (!string.IsNullOrEmpty(Globals.Player.Selected))
		{
			CursorItemSprite.Texture = Database.Instance.GetTexture(Globals.Player.Selected);
			CursorItemSprite.Scale = new Vector2(32 / (float)CursorItemSprite.Texture.GetWidth(),
				32 / (float)CursorItemSprite.Texture.GetHeight());
		}

		if (Building.IsBuilding(Globals.Player.Selected))
		{
			CursorBuildingSprite.Texture = Database.Instance.GetTexture(Globals.Player.Selected);
			_buildingInHand = Building.GetBuilding(Globals.Player.Selected, IRotatable.BuildingOrientation.NorthSouth);
		}
		else
		{
			_buildingInHand = null;
		}

		var shouldShowPowerRange = ShouldShowPowerRange;
		Globals.FactoryScene.TileMap.GetEntities<Pylon>().ForEach(pylon => pylon.ShowRange = shouldShowPowerRange);
		
		if (_buildingInHand is not IRotatable) Globals.Player.SelectionRotationDegrees = 0;
		UpdateLabel();
	}

	private void UpdateLabel()
	{
		var count = Globals.Player.Selected != null ? Globals.PlayerInventory.CountItem(Globals.Player.Selected) : 0;
		CursorItemCount.Text = count != 0 ? count.ToString() : "";
		if (count == 0) Globals.Player.Selected = null;
	}
	
	private void UpdateCursorPosition()
	{
		var cursorPosition = Globals.Player.GetGlobalMousePosition();
		
		// Movement can come from either the cursor moving or the player moving so we just update every frame.
		CursorItemCount.Position = cursorPosition + new Vector2(16, 16);
		CursorItemSprite.Position = cursorPosition;
		
		if (_buildingInHand == null) return;
		CursorBuildingSprite.GlobalPosition = Globals.FactoryScene.TileMap.ToMap(cursorPosition);
		if (_buildingInHand.BuildingSize.X % 2 == 0) CursorBuildingSprite.Position += new Vector2(16, 0);
		if (_buildingInHand.BuildingSize.Y % 2 == 0) CursorBuildingSprite.Position += new Vector2(0, 16);
	}

	private void UpdateCursorSprites()
	{
		// Process mouseover
		if (Globals.Player.Selected != null && Building.IsBuilding(Globals.Player.Selected) && !_motionEventHandled) SelectedBuildingPreview();
		else if (Globals.Player.Selected != null) SelectedItemPreview();
		else
		{
			CursorItemSprite.Visible = false;
			CursorBuildingSprite.Visible = false;
		}

		if (!_motionEventHandled
		    && (Globals.FactoryScene.TileMap.GetBuildingAtMouse() != null ||
		        Globals.FactoryScene.TileMap.GetResourceAtMouse() != null))
		{
			MouseoverEntity();
		}
		else
		{
			CursorEntitySprite.Visible = false;
			_cursorColorTween?.Kill();
			_cursorColorTween = null;
		}
	}
	
	private void SelectedItemPreview()
	{
		CursorBuildingSprite.Visible = false;
		_cursorColorTween?.Kill();
		_cursorColorTween = null;
		CursorItemSprite.Visible = true;
		CursorItemSprite.RotationDegrees = Globals.Player.SelectionRotationDegrees;
		CursorItemSprite.Modulate = Colors.White;
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
		
		if (_buildingInHand is IRotatable rotatable)
			rotatable.Orientation = IRotatable.GetOrientationFromDegrees(Globals.Player.SelectionRotationDegrees);
		
		var canPlace = _buildingInHand.CanPlaceAt(Globals.Player.GetGlobalMousePosition());
		CursorBuildingSprite.SelfModulate = canPlace
			? new Color(0, 1, 0, 0.5f)
			: new Color(1, 0, 0, 0.5f);

		CursorBuildingSprite.GetChildren().ToList().ForEach(child => child.QueueFree());
		if (canPlace && _buildingInHand is Pylon pylon) UpdatePylon(pylon);
	}

	private void UpdatePylon(Pylon pylon)
	{
		var cursorPosition = Globals.Player.GetGlobalMousePosition();
		var mapPosition = Globals.FactoryScene.TileMap.GlobalToMap(cursorPosition);
		var globalPosition = Globals.FactoryScene.TileMap.ToMap(cursorPosition);
		pylon.Update(mapPosition);
		
		pylon.Links.ToList().ForEach(link =>
		{
			CursorBuildingSprite.AddChild(new Line2D 
			{
				Width = Pylon.LineWidth + 1, 
				DefaultColor = Utils.ManaColor,
				Points = [Vector2.Zero, link.GlobalPosition - globalPosition]
			});
		});
		var poly = Pylon.GetPowerRangePolygon();
		poly.Color = poly.Color.Darkened(0.5f);
		CursorBuildingSprite.AddChild(poly);
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
		CursorEntitySprite.SelfModulate = Colors.White;

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
