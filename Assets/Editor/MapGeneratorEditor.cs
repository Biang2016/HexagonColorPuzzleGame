using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    private MapGenerator MG;

    private float HexRadius;
    private float sqrt3 = Mathf.Sqrt(3);

    //六边形坐标，OX和OY为单位向量
    private Vector2 OX;
    private Vector2 OY;

    void OnSceneGUI()
    {
        MG = target as MapGenerator;
        HexRadius = MG.HexRadius;
        OX = new Vector2(1.5f * HexRadius, -sqrt3 / 2 * HexRadius);
        OY = new Vector2(1.5f * HexRadius, sqrt3 / 2 * HexRadius);

        if (Event.current != null)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 10000))
            {
                HexVertex = DrawHexOutline(hit.point, out HexCenterPosition);

                if (Event.current.button == 0 && Event.current.clickCount == 2)
                {
                    if (!AlreadyInstantiate)
                    {
                        MapGrid grid = GameObjectPoolManager.Instance.PoolDict[(GameObjectPoolManager.PrefabNames) Enum.Parse(typeof(GameObjectPoolManager.PrefabNames), MG.CurrentMapGridType.ToString())].AllocateGameObject<MapGrid>(MG.transform);
                        grid.Init(MG.CurrentMapGridColorType,MG.CurrentMapGridType, MG.GridScale, new Vector3(HexCenterPosition.x, MG.transform.position.y, HexCenterPosition.y), grid.name, -1);
                        MG.MapGrids.Add(grid);
                        AlreadyInstantiate = true;
                    }
                }
                else
                {
                    AlreadyInstantiate = false;
                }
            }
        }

        Handles.color = Color.green;
        Handles.DrawPolyLine(HexVertex);
        SceneView.RepaintAll();
    }

    Vector3[] HexVertex = new Vector3[7];
    Vector2 HexCenterPosition = Vector2.zero;
    bool AlreadyInstantiate = false;

    private Vector3[] DrawHexOutline(Vector3 point, out Vector2 hexCenterPosition)
    {
        Vector2 planeExactPosition;
        Vector3[] hexVertex = new Vector3[7];
        planeExactPosition.x = point.x;
        planeExactPosition.y = point.z;
        Vector2 HexPosition = ConvertOrthToHexPosition(planeExactPosition);
        hexCenterPosition = HexPosition.x * OX + HexPosition.y * OY;

        hexVertex[0].x = hexCenterPosition.x - HexRadius;
        hexVertex[0].y = 0;
        hexVertex[0].z = hexCenterPosition.y;

        hexVertex[1].x = hexCenterPosition.x - 0.5f * HexRadius;
        hexVertex[1].y = 0;
        hexVertex[1].z = hexCenterPosition.y + OY.y;

        hexVertex[2].x = hexCenterPosition.x + 0.5f * HexRadius;
        hexVertex[2].y = 0;
        hexVertex[2].z = hexCenterPosition.y + OY.y;

        hexVertex[3].x = hexCenterPosition.x + HexRadius;
        hexVertex[3].y = 0;
        hexVertex[3].z = hexCenterPosition.y;

        hexVertex[4].x = hexCenterPosition.x + 0.5f * HexRadius;
        hexVertex[4].y = 0;
        hexVertex[4].z = hexCenterPosition.y - OY.y;

        hexVertex[5].x = hexCenterPosition.x - 0.5f * HexRadius;
        hexVertex[5].y = 0;
        hexVertex[5].z = hexCenterPosition.y - OY.y;

        hexVertex[6] = hexVertex[0];

        return hexVertex;
    }

    internal Vector2 ConvertOrthToHexPosition(Vector2 orth_Position)
    {
        Vector2 res = Vector2.zero;
        float nADDm;
        float nMINUSm;
        nADDm = orth_Position.x / OX.x;
        nMINUSm = orth_Position.y / OX.y;
        float n;
        float m;
        n = (nADDm + nMINUSm) / 2;
        m = (nADDm - nMINUSm) / 2;

        float n_floor = Mathf.FloorToInt(n);
        float m_floor = Mathf.FloorToInt(m);
        float Local_X = n - n_floor;
        float Local_Y = m - m_floor;

        if (Local_X <= -2 * Local_Y + 1 && Local_Y <= -2 * Local_X + 1)
        {
            res.x = n_floor;
            res.y = m_floor;
        }
        else if (Local_Y >= -2 * Local_X + 2 && Local_X >= -2 * Local_Y + 2)
        {
            res.x = n_floor + 1;
            res.y = m_floor + 1;
        }
        else if (Local_X >= Local_Y)
        {
            res.x = n_floor + 1;
            res.y = m_floor;
        }
        else if (Local_X <= Local_Y)
        {
            res.x = n_floor;
            res.y = m_floor + 1;
        }

        return res;
    }
}