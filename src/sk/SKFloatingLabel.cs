using Godot;

namespace Necromation.sk;

public class SKFloatingLabel
{
    public static void Show(string text, Vector2 position)
    {
        RichTextLabel label = new();
        label.AutowrapMode = TextServer.AutowrapMode.Off;
        label.CustomMinimumSize = new Vector2(1000, 100);
        
        label.Text = "[font_size=10]" + text + "[/font_size]";
        label.GlobalPosition = position;
        label.BbcodeEnabled = true;
        label.ZIndex = 100;
        
        Globals.FactoryScene.AddChild(label);

        var textPositionTween = Globals.Tree.CreateTween();
        textPositionTween.TweenProperty(label, "global_position", label.GlobalPosition + new Vector2(0, -50), 1.0f);
        textPositionTween.TweenCallback(Callable.From(() => label.QueueFree()));
        
        var textColorTween = Globals.Tree.CreateTween();
        textColorTween.TweenProperty(label, "modulate", new Color(1, 1, 1, 0), 1.0f);
    }
}