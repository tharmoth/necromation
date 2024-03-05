public class UnitStats
{
    public string UnitType { get; set; }
    public int Count { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }

    public UnitStats(string unitType)
    {
        UnitType = unitType;
        Count = 0;
        Kills = 0;
        Deaths = 0;
    }

    public void IncrementCount() => Count++;
    public void IncrementKills() => Kills++;
    public void IncrementDeaths() => Deaths++;
}