using Necromation.bridges;

namespace Necromation.map.character.command;

public class StartBattleCommand : Command
{
    public override void Execute(Actor actor, double delta)
    {
        MapBattleBridge.Battle(actor);
    }
}