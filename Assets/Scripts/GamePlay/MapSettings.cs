using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapSettings", menuName = "Map/MapSettings")]
public class MapSettings : ScriptableObject
{
    public Color[] MapGridColors;

    public static HashSet<MapGridColorTypes> PrimaryColorSet = new HashSet<MapGridColorTypes>
    {
        MapGridColorTypes.Red, MapGridColorTypes.Yellow, MapGridColorTypes.Blue,
    };

    public static HashSet<MapGridColorTypes> SecondaryColorSet = new HashSet<MapGridColorTypes>
    {
        MapGridColorTypes.Orange, MapGridColorTypes.Purple, MapGridColorTypes.Green,
    };

    public static Dictionary<MapGridColorTypes, MapGridTypes> MixedMapGridTypeDict = new Dictionary<MapGridColorTypes, MapGridTypes>
    {
        {MapGridColorTypes.Red, MapGridTypes.NormalGrid},
        {MapGridColorTypes.Yellow, MapGridTypes.NormalGrid},
        {MapGridColorTypes.Blue, MapGridTypes.NormalGrid},
        {MapGridColorTypes.Orange, MapGridTypes.Jungle},
        {MapGridColorTypes.Purple, MapGridTypes.Jungle},
        {MapGridColorTypes.Green, MapGridTypes.Jungle},
    };

    public static bool IsDragValid(MapGridColorTypes colorType, MapGridTypes mapGridType)
    {
        if (SecondaryColorSet.Contains(colorType)) return false;
        if (mapGridType != MapGridTypes.HQ) return false;
        return true;
    }
    public static bool IsMoveValid(MapGridColorTypes colorType, MapGridTypes mapGridType)
    {
        if (SecondaryColorSet.Contains(colorType)) return false;
        if (mapGridType != MapGridTypes.HQ) return false;
        return true;
    }

    /// <summary>
    /// Origin and Foreign should be color (not empty)
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="foreign">Foreign should be Primary Colors</param>
    /// <returns></returns>
    public static MapGridColorTypes ColorMix(MapGridColorTypes origin, MapGridColorTypes foreign)
    {
        if (SecondaryColorSet.Contains(origin))
        {
            return origin;
        }
        else if (PrimaryColorSet.Contains(origin))
        {
            switch (origin)
            {
                case MapGridColorTypes.Red:
                {
                    switch (foreign)
                    {
                        case MapGridColorTypes.Red:
                        {
                            return MapGridColorTypes.Red;
                        }
                        case MapGridColorTypes.Yellow:
                        {
                            return MapGridColorTypes.Orange;
                        }
                        case MapGridColorTypes.Blue:
                        {
                            return MapGridColorTypes.Purple;
                        }
                    }

                    break;
                }
                case MapGridColorTypes.Yellow:
                {
                    switch (foreign)
                    {
                        case MapGridColorTypes.Red:
                        {
                            return MapGridColorTypes.Orange;
                        }
                        case MapGridColorTypes.Yellow:
                        {
                            return MapGridColorTypes.Yellow;
                        }
                        case MapGridColorTypes.Blue:
                        {
                            return MapGridColorTypes.Green;
                        }
                    }

                    break;
                }
                case MapGridColorTypes.Blue:
                {
                    switch (foreign)
                    {
                        case MapGridColorTypes.Red:
                        {
                            return MapGridColorTypes.Purple;
                        }
                        case MapGridColorTypes.Yellow:
                        {
                            return MapGridColorTypes.Green;
                        }
                        case MapGridColorTypes.Blue:
                        {
                            return MapGridColorTypes.Blue;
                        }
                    }

                    break;
                }
            }
        }

        return MapGridColorTypes.Red;
    }
}