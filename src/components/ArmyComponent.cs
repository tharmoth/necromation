
using System;
using System.Collections.Generic;
using Necromation.map.character;

public class ArmyComponent
{
    public Action OnBattleLost;
    public List<Commander> Commanders { get; } = [];
}