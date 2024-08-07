using Godot;


public partial class CursorIconManager : Node
{
    private Texture _canGrabCursor = Database.Instance.GetTexture("can_grab");
    private Texture _grabbingCursor = Database.Instance.GetTexture("grabbing");
    private Texture _defaultCursor = Database.Instance.GetTexture("pointing");

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (InteractComponent.MouseOverQueue.Count > 0)
        {
            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                Input.SetCustomMouseCursor(_grabbingCursor);
            }
            else
            {
                Input.SetCustomMouseCursor(_canGrabCursor);
            }
        }
        else
        {
            Input.SetCustomMouseCursor(_defaultCursor);
        }
    }
}