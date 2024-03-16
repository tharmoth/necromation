using System.Collections.Generic;
using Godot;
using Necromation.sk;

namespace Necromation.factory.character.Actions;

public partial class DeconstructAction : Node2D
{
    private Vector2 _startGlobal;
    private Vector2 _endGlobal;
    private bool _dragging = false;
    private bool _deconstructing = false;
    private HashSet<Building> _buildings = new();

    public override void _Process(double delta)
    {
        base._Process(delta);
        QueueRedraw();
        
        if (!_deconstructing || !_dragging) return;

        var tempEnd = GetGlobalMousePosition();
        if (Utils.IsEqualApprox(tempEnd, _endGlobal)) return;
        _endGlobal = tempEnd;
        GetBuildings();
    }

    private void GetBuildings()
    {
        var tilemap = Globals.FactoryScene.TileMap;

        var start = tilemap.GlobalToMap(_startGlobal);
        var end = tilemap.GlobalToMap(GetGlobalMousePosition());
        
        var x = start.X < end.X ? start.X : end.X;
        var y = start.Y < end.Y ? start.Y : end.Y;
        var width = start.X > end.X ? start.X - x : end.X - x;
        var height = start.Y > end.Y ? start.Y - y : end.Y - y;
        
        var buildings = new List<Building>();
        for (var i = x; i < x + width; i++)
        {
            for (var j = y; j < y + height; j++)
            {
                var building = tilemap.GetEntity(new Vector2I(i, j), FactoryTileMap.Building);
                if (building is Building b && !buildings.Contains(b)) buildings.Add(b);
            }
        }

        foreach (var building in _buildings)
        {
            if (!IsInstanceValid(building.OutlineSprite)) continue;
            building.OutlineSprite.Visible = false;
        }
        
        _buildings = new HashSet<Building>(buildings);
        
        foreach (var building in _buildings)
        {
            building.OutlineSprite.Visible = true;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (Input.IsActionJustPressed("deconstruct"))
        {
            Globals.Player.Selected = null;
            _deconstructing = !_deconstructing;
        }

        if (Input.IsActionJustPressed("clear_selection") || Globals.Player.Selected != null)
        {
            _deconstructing = false;
            _dragging = false;
        }
        
        if (!_deconstructing)
        {
            if (!_dragging) return;
            _dragging = false;
            QueueRedraw();
            return;
        }
        
        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (!_dragging)
            {
                _startGlobal = GetGlobalMousePosition();
                _dragging = true;
            }

            QueueRedraw();
        }
        else if (_dragging)
        {
            _dragging = false;
            QueueRedraw();
            foreach (var building in _buildings)
            {
                var delayTween = CreateTween();
                delayTween.TweenInterval(GD.RandRange(0, .5f));
                delayTween.TweenCallback(Callable.From(() => building.Remove(Globals.PlayerInventory, true)));
                building.OutlineSprite.Visible = false;
            }
            _buildings.Clear();
        }
    }

    public override void _Draw()
    {
        base._Draw();
        if (_deconstructing) DrawRect(new Rect2(GetLocalMousePosition(), new Vector2(32, 32)), new Color("a63c45"));
        
        if (!_dragging) return;
        DrawRect(new Rect2(ToLocal(_startGlobal), GetGlobalMousePosition() - _startGlobal), new Color("a63c4560"));
        DrawRect(new Rect2(ToLocal(_startGlobal), GetGlobalMousePosition() - _startGlobal), new Color("a63c45"), false, 5);
    }
}