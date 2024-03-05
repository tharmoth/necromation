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

	private static Main _factoryScene;

	public static Main FactoryScene
	{
		get => _factoryScene ??= LoadScene("res://src/main.tscn") as Main;
	}

	private static Map _mapScene;

	public static Map MapScene
	{
		get => _mapScene ??= LoadScene("res://src/map.tscn") as Map;
	}

	private static Battle _battleScene;

	public static Battle BattleScene
	{
		get => _battleScene ??= LoadScene("res://src/battle.tscn") as Battle;
	}
	
	private static Scene _currentScene;
	private static bool _changingScene = false;

	public static void Register(Scene scene)
	{
		switch (scene)
		{
			case Main main:
				_factoryScene = main;
				break;
			case Map map:
				_mapScene = map;
				break;
			case Battle battle:
				_battleScene = battle;
				break;
			default:
				throw new ArgumentException("Scene is not a valid type", nameof(scene));
		}

		_currentScene ??= scene;
	}

	public static void ChangeToScene(SceneEnum scene)
	{
		if (_changingScene) return;
		_changingScene = true;

		switch (scene)
		{
			case SceneEnum.Factory:
				if (!GodotObject.IsInstanceValid(_factoryScene)) _factoryScene = LoadScene("res://src/main.tscn") as Main;
				ChangeScene(_factoryScene);
				break;
			case SceneEnum.Map:
				if (!GodotObject.IsInstanceValid(_mapScene)) _mapScene = LoadScene("res://src/map.tscn") as Map;
				ChangeScene(_mapScene);
				break;
			case SceneEnum.Battle:
				if (!GodotObject.IsInstanceValid(_battleScene)) _battleScene = LoadScene("res://src/battle.tscn") as Battle;
				ChangeScene(_battleScene);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(scene), scene, null);
		}

		// We need  to wait or this will be called again once the map loads and it sees inputjustpressed.
		Globals.FactoryScene.GetTree().CreateTimer(.1).Timeout += () => _changingScene = false;
	}
	
	private static Scene LoadScene(string path)
	{
		var scene = GD.Load<PackedScene>(path).Instantiate<Scene>();
		
		scene.Visible = false;
		scene.ProcessMode = Node.ProcessModeEnum.Disabled;
		scene.Camera.Enabled = false;
		scene.Gui.Visible = false;
		
		SceneTree.Root.CallDeferred("add_child", scene);
		return scene;
	}

	private static void ChangeScene(Scene to)
	{
		_currentScene ??= SceneTree.Root.GetChildren().OfType<Scene>().FirstOrDefault();
		_currentScene.OnClose();
		_currentScene.Visible = false;
		_currentScene.ProcessMode = Node.ProcessModeEnum.Disabled;
		_currentScene.Camera.Enabled = false;
		_currentScene.Gui.Visible = false;
		
		to.Visible = true;
		to.ProcessMode = Node.ProcessModeEnum.Inherit;
		to.Camera.Enabled = true;
		to.Gui.Visible = true;

		_currentScene = to;
		_currentScene.OnOpen();
	}
}
