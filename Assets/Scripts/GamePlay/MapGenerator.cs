using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MapGenerator : MonoBehaviour
{
    public float HexRadius = 1.0f; //六边形外接圆半径
    public float BorderRatio = 0.1f; //六边形间距
    public float GridScale => HexRadius * (1 - BorderRatio) / 10f; //六边形间距

    public MapGridTypes CurrentMapGridType;
    public MapGridColorTypes CurrentMapGridColorType;

    public List<MapGrid> MapGrids = new List<MapGrid>();

    public void ClearCurrentBoard()
    {
        try
        {
            foreach (MapGrid mapGrid in MapGrids)
            {
                if (mapGrid) mapGrid.PoolRecycle();
            }
        }
        catch
        {
            foreach (MapGrid mapGrid in MapGrids)
            {
                if (mapGrid) DestroyImmediate(mapGrid.gameObject);
            }
        }

        MapGrids.Clear();
    }
}