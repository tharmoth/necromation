using Godot;
using System;
using System.Linq;
using Necromation;
using Necromation.map;

public class SceneManager
{
	public enum SceneEnum
	{
		Factory,
		Map,
		Battle
	}

	private static SceneTree _sceneTree;
	public static SceneTree SceneTree => _sceneTree ??= Engine.GetMainLoop() as SceneTree;

	private static FactoryScene _factoryScene;

	public static FactoryScene FactoryScene
	{
		get => _factoryScene ??= LoadScene("res://src/factory/factory.tscn") as FactoryScene;
	}

	private static MapScene _mapSceneScene;

	public static MapScene MapScene
	{
		get => _mapSceneScene ??= LoadScene("res://src/map/map.tscn") as MapScene;
	}

	private static BattleScene _battleSceneScene;

	public static BattleScene BattleScene
	{
		get => _battleSceneScene ??= LoadScene("res://src/battle/battle.tscn") as BattleScene;
	}
	
	private static Scene _currentScene;
	private static bool _changingScene = false;
	
	private static readonly ColorRect TransitionFade = new()
	{
		Color = new Color("00000000"),
		AnchorsPreset = (int) Control.LayoutPreset.FullRect
	};

	public static void Register(Scene scene)
	{
		switch (scene)
		{
			case FactoryScene main:
				_factoryScene = main;
				break;
			case MapScene map:
				_mapSceneScene = map;
				break;
			case BattleScene battle:
				_battleSceneScene = battle;
				break;
			default:
				throw new ArgumentException("Scene is not a valid type", nameof(scene));
		}

		_currentScene ??= scene;
		_currentScene.OnOpen();
	}

	public static void ChangeToScene(SceneEnum scene)
	{
		if (_changingScene) return;
		_changingScene = true;
		
		if (TransitionFade.GetParent() == null)
		{
			var canvasLayer = new CanvasLayer {Layer = 100};
			canvasLayer.AddChild(TransitionFade);
			TransitionFade.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			SceneTree.Root.CallDeferred("add_child", canvasLayer);
		}

		TransitionFade.MouseFilter = Control.MouseFilterEnum.Stop;
		var tween = SceneTree.Root.CreateTween();
		tween.TweenProperty(TransitionFade, "color:a", 1.0f, 0.2);
		tween.TweenCallback(Callable.From(() =>
		{
			switch (scene)
			{
				case SceneEnum.Factory:
					if (!GodotObject.IsInstanceValid(_factoryScene)) _factoryScene = LoadScene("res://src/factory/factory.tscn") as FactoryScene;
					ChangeScene(_factoryScene);
					break;
				case SceneEnum.Map:
					if (!GodotObject.IsInstanceValid(_mapSceneScene)) _mapSceneScene = LoadScene("res://src/map/map.tscn") as MapScene;
					ChangeScene(_mapSceneScene);
					break;
				case SceneEnum.Battle:
					if (!GodotObject.IsInstanceValid(_battleSceneScene)) _battleSceneScene = LoadScene("res://src/battle/battle.tscn") as BattleScene;
					ChangeScene(_battleSceneScene);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(scene), scene, null);
			}
		}));
		tween.TweenInterval(0.5f);
		tween.TweenProperty(TransitionFade, "color:a", 0.0f, 0.2);
		tween.TweenCallback(Callable.From(() => TransitionFade.MouseFilter = Control.MouseFilterEnum.Ignore));

		// We need  to wait or this will be called again once the map loads and it sees inputjustpressed.
		SceneTree.CreateTimer(.1).Timeout += () => _changingScene = false;
	}
	
	private static Scene LoadScene(string path)
	{
		var scene = GD.Load<PackedScene>(path).Instantiate<Scene>();
		
		scene.Visible = false;
		scene.ProcessMode = Node.ProcessModeEnum.Disabled;
		scene.Camera.Enabled = false;
		if (scene.Gui != null) scene.Gui.Visible = false;
		
		SceneTree.Root.CallDeferred("add_child", scene);
		return scene;
	}

	private static void ChangeScene(Scene to)
	{
		_currentScene ??= SceneTree.Root.GetChildren().OfType<Scene>().FirstOrDefault();
		if (_currentScene != null)
		{
			_currentScene.OnClose();
			_currentScene.Visible = false;
			_currentScene.ProcessMode = Node.ProcessModeEnum.Disabled;
			_currentScene.Camera.Enabled = false;
			_currentScene.Gui.Visible = false;
		}
		
		to.Visible = true;
		to.ProcessMode = Node.ProcessModeEnum.Inherit;
		to.Camera.Enabled = true;
		if (to.Gui != null) to.Gui.Visible = true;

		_currentScene = to;
		_currentScene.OnOpen();
	}
}
