using System.Collections.Generic;
using System.Linq;
using Necromation.map.character;

namespace Necromation.bridges;

public static class MapBattleBridge
{
    public static List<Commander> Commanders { get; } = [];
    
    private static Actor _enemy;
    
    public static void Battle(Actor enemy)
    {
	    _enemy = enemy;
	    
        var enemyCommanders = enemy.GetComponent<ArmyComponent>().Commanders;

        var player = Globals.MapScene.Player;
        var playerCommanders = player.GetComponent<ArmyComponent>().Commanders;
		
        var commanders = enemyCommanders.Union(playerCommanders).ToList();
		
        Commanders.Clear();
        Commanders.AddRange(commanders);

        SceneManager.ChangeToScene(SceneManager.SceneEnum.Battle);
    }
    
    public static void EndBattle(string winner)
	{
		if (winner == "Player")
		{
			_enemy.GetComponent<ArmyComponent>().OnBattleLost?.Invoke();
		}
		else
		{
			Globals.MapScene.Player.GetComponent<ArmyComponent>().OnBattleLost?.Invoke();
		}
	}
}