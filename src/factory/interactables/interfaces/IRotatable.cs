using System;
using Godot;

namespace Necromation.interactables.interfaces;

public interface IRotatable
{
    public enum BuildingOrientation
    {
        NorthSouth,
        EastWest,
        SouthNorth,
        WestEast
    }

    public BuildingOrientation Orientation { get; set; }
    
    public static BuildingOrientation GetOppositeOrientation(BuildingOrientation orientation)
    {
        return orientation switch
        {
            BuildingOrientation.NorthSouth => BuildingOrientation.SouthNorth,
            BuildingOrientation.EastWest => BuildingOrientation.WestEast,
            BuildingOrientation.SouthNorth => BuildingOrientation.NorthSouth,
            BuildingOrientation.WestEast => BuildingOrientation.EastWest,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public static BuildingOrientation GetOrientationFromDegrees(int degrees) => degrees switch
    {
        0 => BuildingOrientation.NorthSouth,
        90 => BuildingOrientation.EastWest,
        180 => BuildingOrientation.SouthNorth,
        270 => BuildingOrientation.WestEast,
        _ => throw new ArgumentOutOfRangeException(nameof(degrees), degrees, null)
    };
    
    public static int GetDegreesFromOrientation(BuildingOrientation orientation) => orientation switch
    {
        BuildingOrientation.NorthSouth => 0,
        BuildingOrientation.EastWest => 90,
        BuildingOrientation.SouthNorth => 180,
        BuildingOrientation.WestEast => 270,
        _ => throw new ArgumentOutOfRangeException()
    };
    
    public static float GetRadiansFromOrientation(BuildingOrientation orientation) => Mathf.DegToRad(GetDegreesFromOrientation(orientation));
}