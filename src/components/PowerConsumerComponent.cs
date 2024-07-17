using Godot;

public class PowerConsumerComponent : IPowerConsumer
{
    /**************************************************************************
     * Public Variables                                                       *
     **************************************************************************/
    #region IPowerConsumer Implementation
    public float Energy { get; set; }
    public float EnergyMax => 100;
    public float Power => 10;
    public bool Disconnected
    {
        set
        {
            if (value)
            {
                OnDisconnect();
            }
            else
            {
                OnConnect();
            }
        }
    }
    #endregion
    
    /**************************************************************************
     * Private Variables                                                      *
     **************************************************************************/
    private readonly Sprite2D _disconnectedSprite = new()
    {
        ZIndex = 100,
        Texture = Database.Instance.GetTexture("Disconnected"),
        Visible = false
    };
    private Tween _disconnectedTween;

    /**************************************************************************
     * Constructor                                                            *
     **************************************************************************/
    public PowerConsumerComponent(Node2D parent)
    {
        _disconnectedSprite.Position = new Vector2(0, -10);
        _disconnectedSprite.ScaleToSize(Vector2.One * 32.0f);
        parent.AddChild(_disconnectedSprite);
    }
    
    /**************************************************************************
     * Public Methods                                                       *
     **************************************************************************/
    public bool DrawPower(double delta)
    {
        if (Energy < Power * (float) delta) return false;
        Energy -= Power * (float) delta;
        return true;
    }

    /**************************************************************************
     * Private Functions                                                      *
     * ************************************************************************/
    private void OnDisconnect()
    {
        _disconnectedSprite.Visible = true;
        if (_disconnectedTween != null) return;
        _disconnectedTween = _disconnectedSprite.CreateTween();
        _disconnectedTween.TweenProperty(_disconnectedSprite, "modulate", Colors.Transparent, 0.5f);
        _disconnectedTween.TweenProperty(_disconnectedSprite, "modulate", Colors.White, 0.5f);
        _disconnectedTween.SetLoops();
    }

    private void OnConnect()
    {
        _disconnectedSprite.Visible = false;
        _disconnectedTween?.Kill();
        _disconnectedTween = null;
    }
}