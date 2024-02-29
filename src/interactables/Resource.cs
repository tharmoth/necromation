using Godot;
using Necromation.sk;

namespace Necromation;

public partial class Resource : Node2D, LayerTileMap.IEntity
{
    [Export] public string Type { get; set; } = "Stone";
    [Export] private float _duration = 1.0f;
    public string ItemType => Type;

    private Tween _tween;
    private Sprite2D _sprite = new();
    private ProgressBar _progressBar = new();
    
    public Resource(string itemType)
    {
        Type = itemType;
        _sprite.Texture = Globals.Database.GetTexture(Type);
        AddChild(_sprite);
        
        _progressBar.Scale = new Vector2(0.25f, 0.25f);
        _progressBar.Position -= new Vector2(16, -6);
        _progressBar.Size = new Vector2(128, 28);
        _progressBar.ShowPercentage = false;
        _progressBar.Visible = false;
        AddChild(_progressBar);
    }
        
    public override void _Ready()
    {
        base._Ready();
        AddToGroup("resources");
        AddToGroup("Persist");
        Globals.TileMap.AddEntity(GlobalPosition, this, BuildingTileMap.Resource);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Globals.TileMap.RemoveEntity(this);
    }

    public void Interact(Inventory playerInventory)
    {
        if (!CanInteract() || _tween != null) return;
        
        _progressBar.Value = 0;
        _progressBar.Visible = true;

        _tween?.Kill();
        _tween = GetTree().CreateTween();
        _tween.TweenProperty(_progressBar, "value", 100, _duration);
        _tween.TweenCallback(Callable.From(() => Complete(playerInventory)));
    }

    protected void Complete(Inventory playerInventory)
    {
        _tween = null;
        _progressBar.Value = 0;
        _progressBar.Visible = false;
        playerInventory.Insert(Type);
    }

    public virtual bool CanInteract()
    {
        return _tween == null;
    }

    public bool Interacting()
    {
        return _tween != null;
    }
    
    public void Cancel()
    {
        _tween?.Kill();
        _tween = null;
        _progressBar.Value = 0;
        _progressBar.Visible = false;
    }

    public bool CanRemove()
    {
        return false;
    }
    
    #region Save/Load
    /******************************************************************
     * Save/Load                                                      *
     ******************************************************************/
    public Godot.Collections.Dictionary<string, Variant> Save()
    {
        return new Godot.Collections.Dictionary<string, Variant>()
        {
            { "ItemType", ItemType },
            { "PosX", Position.X }, // Vector2 is not supported by JSON
            { "PosY", Position.Y },
        };
    }
	
    public static void Load(Godot.Collections.Dictionary<string, Variant> nodeData)
    {
        Resource resource = new Resource(nodeData["ItemType"].ToString());
        resource.GlobalPosition = new Vector2((float)nodeData["PosX"], (float)nodeData["PosY"]);
        Globals.FactoryScene.AddChild(resource);
    }
    #endregion
}