using System.Collections.Generic;
using System.Linq;
using Necromation;
using Necromation.map.character.command;

public class AttackState : IState
{
    public required Actor Actor { private get; init; }

    public List<Command> Commands
    {
        get
        {
            var player = Globals.MapScene.Actors.First(a => a.GetComponent<StatsComponent>().Team == "Player");
            var commands = new List<Command> { new MoveCommand { TargetPosition = player.GlobalPosition} };
            if (ShouldInteract(Actor, player))
            {
                commands.Add(new StartBattleCommand());
            }

            return commands;
        }
    }

    public IState NextState => this;
    public void _Process(double delta) { }

    private bool ShouldInteract(Actor actor, Actor targetActor)
    {
        if (targetActor == null) return false;
        var interactDistance = actor.GetComponent<StatsComponent>().InteractDistance;
        return targetActor.GlobalPosition.DistanceTo(actor.GlobalPosition) < interactDistance;
    }
}