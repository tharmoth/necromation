using Godot;
using System;

public abstract partial class Scene : Node2D
{
    // We need the initalize the scene in the SceneManager for when this is the first scene to be loaded.
    public override void _EnterTree()
    {
        base._EnterTree();
        SceneManager.Register(this);
    }

    // The Scenes GUI node. This requres that the scene have a child node named %GUI. Used to access the GUI canvas layer
    // of the scene. Made not Visible by the SceneManager when the scene is not the current scene.
    public abstract CanvasLayer Gui { get; }

    // The Scenes Camera2D node. This requres that the scene have a child node named %Camera2D. Used to access the camera
    // of the scene. Disabled by the SceneManager when the scene is not the current scene.
    public Camera2D Camera => GetNode<Camera2D>("%Camera2D");

    // Updates the data in a scene in relation to other scenes. Called by the SceneManager whenever ChangeScene is called
    // on this scene.
    public abstract void OnOpen();
    
    // Updates the data in other scenes in relation to this scene. Called by the SceneManager whenever ChangeScene is called
    // on another scene while this one was open.
    public abstract void OnClose();
}
